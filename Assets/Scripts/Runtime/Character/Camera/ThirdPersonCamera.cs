using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace GP
{
    public class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField, BoxGroup("Camera Info", showLabel: false), LabelText("相机跟踪目标:")]
        [FormerlySerializedAs("m_CameraTrackTarget")]
        private Transform m_TrackingTarget;
        [SerializeField, BoxGroup("Camera Info", showLabel: false), LabelText("相机跟踪目标距离:"), Range(1f, 20f)]
        [FormerlySerializedAs("m_ByTrackTargetDistance")]
        private float m_TrackingDistance = 3f;
        [SerializeField, BoxGroup("Camera Info", showLabel: false), LabelText("相机跟踪目标偏移量:")]
        [FormerlySerializedAs("m_CameraTrackTargetOffset")]
        private Vector3 m_TrackingOffset;

        [SerializeField, BoxGroup("Camera Settings", showLabel: false), LabelText("相机灵敏度:")]
        private Vector2 m_CameraSensitivity = new Vector2(3f, 3f);
        [SerializeField, BoxGroup("Camera Settings", showLabel: false), LabelText("相机最大最小俯仰角:")]
        private Vector2 m_CameraMinMaxPitch = new Vector2(-55f, 60f);
        [SerializeField, BoxGroup("Camera Settings", showLabel: false), LabelText("相机旋转平滑度:")]
        private float m_CameraRotationSmooth = 15f;

        [SerializeField, BoxGroup("Camera Collision", showLabel: false), LabelText("是否启用相机碰撞检测:")]
        private bool m_EnableCameraCollision = true;
        [SerializeField, BoxGroup("Camera Collision", showLabel: false), ShowIf(nameof(m_EnableCameraCollision)), LabelText("相机碰撞检测半径:")]
        private float m_CameraCollisionRadius = .2f;
        [SerializeField, BoxGroup("Camera Collision", showLabel: false), ShowIf(nameof(m_EnableCameraCollision)), LabelText("相机碰撞检测层:")]
        private LayerMask m_CameraCollisionMask;

        private GameInputManager m_GameInputManager;

        private Transform m_CameraPivot;
        private bool m_IsFrozen;
        private Vector2 m_CameraInput;
        private float m_CameraYaw; // 控制相机沿 Y 轴的旋转，偏航角
        private float m_CameraPitch; // 控制相机沿 X 轴的旋转，俯仰角
        private Quaternion m_CameraRotation; // 相机朝向

        public Transform MainCamera { get; private set; }

        private void Awake()
        {
            m_GameInputManager = GameInputManager.Instance;

            MainCamera = transform;
            m_CameraPivot = GetComponentInChildren<Camera>()?.transform;
            if (!m_CameraPivot && transform.childCount > 0)
                m_CameraPivot = transform.GetChild(0);
            m_CameraRotation = transform.rotation;
            m_CameraYaw = transform.eulerAngles.y;
            m_CameraPitch = -AngleUtils.NormalizeAngle180(transform.eulerAngles.x);

            SetFrozen(false);
        }

        private void Update()
        {
            ReadCameraInput();
        }

        private void LateUpdate()
        {
            ProcessCameraPosition(); // 处理相机的位置
            ProcessCameraCollision(); // 处理相机的碰撞
        }

        public void SetTrackingTarget(Transform cameraTarget)
        {
            if (!cameraTarget) return;

            m_TrackingTarget = cameraTarget;
        }

        public void SetFrozen(bool isFreeze)
        {
            m_IsFrozen = isFreeze;
        }

        private void ReadCameraInput()
        {
            if (m_IsFrozen) return;
            if (GameManager.GameMode != GameMode.Game) return;
            if (!m_GameInputManager) return;

            m_CameraInput = m_GameInputManager.LookInput.ReadInputValue();

            m_CameraYaw += m_CameraInput.x * .02f * m_CameraSensitivity.x;
            m_CameraPitch += m_CameraInput.y * .02f * m_CameraSensitivity.y;

            // 限制 m_CameraYaw 的范围在 0 - 360°
            m_CameraYaw = AngleUtils.NormalizeAngle360(m_CameraYaw);
            // 限制 m_CameraPitch 的范围在 m_CameraMinMaxPitch.x - m_CameraMinMaxPitch.y
            m_CameraPitch = Mathf.Clamp(m_CameraPitch, m_CameraMinMaxPitch.x, m_CameraMinMaxPitch.y);

            var desiredRotation = Quaternion.Euler(-m_CameraPitch, m_CameraYaw, 0f);

            m_CameraRotation = Quaternion.Slerp(m_CameraRotation, desiredRotation, Time.smoothDeltaTime * m_CameraRotationSmooth);

            MainCamera.rotation = m_CameraRotation;
        }

        private void ProcessCameraPosition()
        {
            if (!m_TrackingTarget) return;

            var desiredCameraPos = m_TrackingTarget.position - MainCamera.forward * m_TrackingDistance; // 相机跟踪目标预定位置

            MainCamera.position = desiredCameraPos + m_TrackingOffset; // 设置相机最终所处位置

            // Note:如果后期需要添加下蹲功能，那么此处处理下蹲后相机位置的偏移量
        }

        private void ProcessCameraCollision()
        {
            if (!m_TrackingTarget || !m_CameraPivot) return;
            if (!m_EnableCameraCollision)
            {
                m_CameraPivot.position = MainCamera.position;
                return;
            }

            // 取起点，相机追踪目标的位置往后移动 _radius 的距离(目的是防止相机进入角色模型内部)
            var origin = Vector3.MoveTowards(m_TrackingTarget.position, MainCamera.position, m_CameraCollisionRadius);

            var direction = MainCamera.position - origin; // 取方向，即原点到相机的方向

            var targetDistance = direction.magnitude;
            if (Cast(origin, direction, m_CameraCollisionRadius, m_CameraCollisionMask, out var hit))
                targetDistance = hit.distance;

            m_CameraPivot.position = Vector3.MoveTowards(origin, MainCamera.position, targetDistance);
        }

        private static bool Cast(Vector3 origin, Vector3 direction, float radius, LayerMask layerMask, out RaycastHit hit)
        {
            if (radius <= float.Epsilon)
            {
                return Physics.Raycast(
                    origin,
                    direction.normalized,
                    out hit,
                    direction.magnitude,
                    layerMask,
                    QueryTriggerInteraction.Ignore
                );
            }

            return Physics.SphereCast(
                origin,
                radius,
                direction.normalized,
                out hit,
                direction.magnitude,
                layerMask,
                QueryTriggerInteraction.Ignore
            );
        }
    }
}
