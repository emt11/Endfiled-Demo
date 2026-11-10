using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace GP
{
    [RequireComponent(typeof(CharacterController))]
    public class ThirdPersonLocomotion : MonoBehaviour
    {
        private static readonly Vector3 s_HorizontalAxisMask = new Vector3(1f, 0f, 1f);
        public const float DefaultGravity = -9.82f;
        private const float GroundDetectionDistance = 1.2f;
        private ThirdPersonCamera m_MainCamera;
        private CharacterController m_CharacterController;
        private GameInputManager m_GameInputManager;
        private AnimationSource m_AnimationSource;

        private Vector3 m_MovementDirection;
        private Vector3 m_VerticalDirection;
        private Vector2 m_MovementInput2D;
        private Vector3 m_MovementInput3D;
        private bool m_FreezeInput;

        private float m_LocomotionSpeed;
        private float m_VerticalVelocity;
        private float m_AdditionalVerticalAcceleration;
        private float m_LocomotionGravityDrag;

        private bool m_DisablePhysics;
        private bool m_IsGrounded;
        private bool m_EnableGroundCheck = true;
        private bool m_UseGravity;
        private Vector3 m_GroundNormal = Vector3.up;
        private bool m_IsOnSteepSlope;
        private RaycastHit m_SlopeHit;

        private float m_OriginHeight;
        private float m_OriginRadius;
        private Vector3 m_OriginCenter;

        [SerializeField, BoxGroup("LocomotionAddOn", showLabel: false), LabelText("运动模式:")]
        private LocomotionAddOnManager m_LocomotionAddOnManager = new();

        [SerializeField, BoxGroup("Speed", showLabel: false), LabelText("奔跑速度:"), HorizontalGroup("Speed/SpeedHorizontal")]
        private float m_RunSpeed;
        [SerializeField, BoxGroup("Speed", showLabel: false), LabelText("冲刺速度:"), HorizontalGroup("Speed/SpeedHorizontal")]
        private float m_SprintSpeed;
        [SerializeField, BoxGroup("Speed", showLabel: false), LabelText("空气阻力系数:")]
        private float m_AirDrag = 4f;
        [SerializeField, BoxGroup("Speed", showLabel: false), LabelText("空气最大下落速度:")]
        private float m_AirTerminalSpeed = 20f;

        [SerializeField, BoxGroup("Grounded", showLabel: false)]
        private float m_GroundCheckRadius = .2f;
        [SerializeField, BoxGroup("Grounded", showLabel: false)]
        private LayerMask m_GroundLayerMask;
        [SerializeField, BoxGroup("Grounded", showLabel: false)]
        private Vector3 m_GroundCheckCenterOffset;

        public float NormalizedLocomotionSpeed { get; private set; }
        public Transform Character { get; private set; }
        public float VerticalVelocity => m_VerticalVelocity;
        public LocomotionAddOnManager LocomotionAddOnManager => m_LocomotionAddOnManager;

        private void Awake()
        {
            m_GameInputManager = GameInputManager.Instance;
            Character = transform;
            m_CharacterController = GetComponent<CharacterController>();
            m_CharacterController.hideFlags = HideFlags.HideInInspector;
            m_AnimationSource = GetComponentInChildren<AnimationSource>();

            m_OriginCenter = m_CharacterController.center;
            m_OriginHeight = m_CharacterController.height;
            m_OriginRadius = m_CharacterController.radius;

            m_LocomotionAddOnManager?.Initialize(this); // 初始化额外移动模式
        }

        private void Start()
        {
            InitializeCamera();

            SetGravityEnabled(true);
            m_LocomotionGravityDrag = m_AirDrag;
            m_AdditionalVerticalAcceleration = 0f;
        }

        private void Update()
        {
            CheckGround();
            ReadMovementInput();
            m_LocomotionAddOnManager?.RunningLocomotion?.LocomotionAddOnTick();
            LocomotionGravity();
            UpdateMovement();
        }

        private void OnDestroy()
        {
            m_LocomotionAddOnManager?.OnDestroy();
        }

        public void SetGroundCheckEnabled(bool enable)
        {
            m_EnableGroundCheck = enable;
        }

        public void SetInputFrozen(bool freezeInput)
        {
            m_FreezeInput = freezeInput;

            if (m_FreezeInput)
            {
                m_MovementInput2D = Vector2.zero;
                m_MovementInput3D = Vector2.zero;
                m_MovementDirection = Vector3.zero;
                m_VerticalDirection = Vector3.zero;
            }
        }

        public void SetGravityEnabled(bool useLocomotionGravity)
        {
            m_UseGravity = useLocomotionGravity;
            m_VerticalVelocity = m_UseGravity ? m_VerticalVelocity : 0f;
        }

        public (float height, float radius, Vector3 center) GetCharacterColliderSize()
        {
            return (m_OriginHeight, m_OriginRadius, m_OriginCenter);
        }

        public void SetCharacterColliderSize(float? height = null, float? radius = null, Vector3? center = null)
        {
            m_CharacterController.center = center ?? m_OriginCenter;
            m_CharacterController.radius = radius ?? m_OriginRadius;
            m_CharacterController.height = height ?? m_OriginHeight;
        }

        public void SetAdditionalVerticalAcceleration(float acceleration)
        {
            m_AdditionalVerticalAcceleration = acceleration;
        }

        public void SetGravityDrag(float? drag = null)
        {
            m_LocomotionGravityDrag = Mathf.Max(0f, drag ?? m_AirDrag);
        }

        public void SetCharacterPosition(Vector3 position)
        {
            bool controllerEnabled = m_CharacterController.enabled;
            m_CharacterController.enabled = false;
            transform.position = position;
            m_CharacterController.enabled = controllerEnabled;
        }

        public void SetCharacterRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
        }

        public void Move(Vector3 direction)
        {
            DoMove(direction);
        }

        public void SetPhysicsDisabled(bool disablePhysics)
        {
            m_DisablePhysics = disablePhysics;
        }

        private void InitializeCamera()
        {
            var mainCamera = Camera.main;
            if (!mainCamera) return;

            m_MainCamera = mainCamera.GetComponentInParent<ThirdPersonCamera>();
        }

        private void ReadMovementInput()
        {
            if (!m_GameInputManager || m_FreezeInput || m_IsOnSteepSlope)
            {
                m_MovementInput2D = Vector2.zero;
                m_MovementInput3D = Vector3.zero;
                return;
            }

            m_MovementInput2D = m_GameInputManager.MovementInput.ReadInputValue();
            m_MovementInput3D.Set(m_MovementInput2D.x, 0f, m_MovementInput2D.y);
        }

        private void UpdateMovement()
        {
            // 更新运动速度
            m_LocomotionSpeed = m_IsGrounded ? m_RunSpeed : m_RunSpeed * .5f;
            var desiredMovementDirection = GetMovementDirection(); // 玩家正在按着移动键

            // 检查当前是否在加速阶段
            var isIncreasing = desiredMovementDirection.sqrMagnitude > m_MovementDirection.sqrMagnitude;
            var acceleration = isIncreasing ? 35f : 15f;

            m_MovementDirection = Vector3.Lerp(m_MovementDirection, desiredMovementDirection, acceleration * Time.smoothDeltaTime);

            if (isIncreasing)
            {
                // 如果此时正在加速，那么就根据当前移动方向和目标移动方向的投影，来计算移动方向
                // 投影的大小不能大于目标移动方向的大小，否则会超出目标移动方向的范围
                var direction = Vector3.Project(m_MovementDirection, desiredMovementDirection);
                m_MovementDirection = direction.sqrMagnitude < desiredMovementDirection.sqrMagnitude
                    ? direction
                    : desiredMovementDirection; // 判断当前移动方向与目标方向是否一致，如果不一致则继续以当前方向移动
            }
            else
            {
                // 如果当前移动方向的大小不小于等于 0，说明方向量还有动能，继续移动
                m_MovementDirection = Mathf.Abs(m_MovementDirection.sqrMagnitude) > .01f
                    ? m_MovementDirection
                    : Vector3.zero;
            }

            var newDirection = m_MovementDirection;

            newDirection *= m_LocomotionSpeed;

            newDirection.Scale(s_HorizontalAxisMask); // 把 Y 轴方向上的量归零.只留下 x,z 方向上的值 new Vector3(1,0,1)

            NormalizedLocomotionSpeed = m_MovementDirection.magnitude;

            if (m_LocomotionAddOnManager.LocomotionMode == LocomotionMode.None)
                newDirection = ProjectMovementOnGround(newDirection);

            var displacement = newDirection * Time.deltaTime;
            if (m_AnimationSource)
            {
                // Animator 提供的根运动已经是本帧位移，不能再次乘以 deltaTime
                displacement = Vector3.Lerp(displacement, m_AnimationSource.RootMotion.PositionDelta,
                    m_AnimationSource.RootMotionWeight);
            }

            DoMove(displacement);
            UpdateRotation();
        }

        private void UpdateRotation()
        {
            if (m_AnimationSource && m_AnimationSource.WarpingRunning) return;
            var dir = m_MovementInput3D.sqrMagnitude > 0f
                ? GetMovementDirection()
                : Character.TransformDirection(Vector3.forward);

            var targetRotation = Quaternion.LookRotation(dir);

            var srcRotation = Character.rotation;

            var dstRotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);

            var rotation = Quaternion.Slerp(srcRotation, dstRotation, 15f * Time.smoothDeltaTime);

            if (m_AnimationSource)
            {
                var rootRotation = srcRotation * m_AnimationSource.RootMotion.RotationDelta;
                rotation = Quaternion.Slerp(rotation, rootRotation, m_AnimationSource.RootMotionWeight);
            }

            Character.rotation = rotation;
        }

        private Vector3 ProjectMovementOnGround(Vector3 direction)
        {
            return Vector3.ProjectOnPlane(direction, m_GroundNormal);
        }

        private Vector3 GetMovementDirection()
        {
            var cameraRotation = m_MainCamera
                ? Quaternion.Euler(0f, m_MainCamera.MainCamera.rotation.eulerAngles.y, 0f)
                : Quaternion.identity; // 取相机的水平旋转量(Y轴旋转)

            // 把输入方向旋转到世界方向
            // 把这个向量绕 Y 轴旋转相机当前的旋转角度，得到世界方向的移动方向
            var moveDirection = cameraRotation * m_MovementInput3D;

            moveDirection.Scale(s_HorizontalAxisMask); // 强制清除 Y 轴的移动量，避免角色在移动时垂直移动(防御性代码，避免以后改动哪来影响了角色的移动)
            moveDirection.Normalize(); // 强制归一化，避免前期计算影响了向量的大小

            // 恢复原始输入强度
            // 相当于恢复手柄摇杆的轻推效果
            return moveDirection * m_MovementInput3D.magnitude;
        }

        private void LocomotionGravity()
        {
            if (!m_UseGravity) return;

            float netAccel = m_AdditionalVerticalAcceleration + DefaultGravity;

            // 施加重力
            m_VerticalVelocity += netAccel * Time.smoothDeltaTime;

            // 指数阻力（空气阻力）利用指数衰减来模拟空气阻力的效果，从而实现角色在空中的下落速度减慢
            m_VerticalVelocity *= Mathf.Exp(-m_LocomotionGravityDrag * Time.smoothDeltaTime);

            // 限制最大下落速度
            m_VerticalVelocity = Mathf.Max(m_VerticalVelocity, -m_AirTerminalSpeed);

            if (m_LocomotionAddOnManager.LocomotionMode == LocomotionMode.None && m_IsGrounded)
                m_VerticalVelocity = -2f;

            m_VerticalDirection = Vector3.up * (m_VerticalVelocity * Time.smoothDeltaTime);

            DoMove(m_VerticalDirection);
        }

        private void CheckGround()
        {
            if (!m_EnableGroundCheck)
            {
                m_IsGrounded = true;
                m_IsOnSteepSlope = false;
                m_GroundNormal = Vector3.up;
                return;
            }

            m_IsGrounded = Physics.CheckSphere(Character.position + m_GroundCheckCenterOffset
                , m_GroundCheckRadius, m_GroundLayerMask, QueryTriggerInteraction.Ignore);

            // 坡道检测
            if (Physics.Raycast(Character.position + m_GroundCheckCenterOffset, Vector3.down, out m_SlopeHit,
                    GroundDetectionDistance, m_GroundLayerMask, QueryTriggerInteraction.Ignore))
            {
                m_GroundNormal = m_SlopeHit.normal;

                var angle = Vector3.Angle(m_SlopeHit.normal, Vector3.up);

                m_IsOnSteepSlope = angle > m_CharacterController.slopeLimit;

                if (!m_IsOnSteepSlope) return;

                // 计算下坡方向
                Vector3 downSlope = Vector3.ProjectOnPlane(Physics.gravity, m_GroundNormal).normalized;
                var slideDir = downSlope * (5f * Time.smoothDeltaTime);
                DoMove(slideDir);
            }
            else
            {
                // 避免角色上一帧还在坡上，下一秒浮空了。检测不到地面时，可能会导致更新不及时导致角色还以为在坡道上滑行
                m_IsOnSteepSlope = false;
                m_GroundNormal = Vector3.up;
            }
        }

        private void DoMove(Vector3 direction)
        {
            if (m_AnimationSource && m_AnimationSource.WarpingRunning) return;
            if (m_DisablePhysics) return;

            m_CharacterController.Move(direction);
        }

        private void OnDrawGizmos()
        {
            if (!Character)
                Character = transform;

#if UNITY_EDITOR
            Handles.Label(Character.position + Vector3.up * 1.5f, "角色移动控制器");
#endif
            Gizmos.DrawWireSphere(Character.position + m_GroundCheckCenterOffset, m_GroundCheckRadius);
            m_LocomotionAddOnManager?.RunningLocomotion?.OnDrawGizmos();
        }

    }
}
