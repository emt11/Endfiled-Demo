using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace GP
{
    [Serializable]
    public class SwimmingLocomotion : ILocomotionAddOn, ITrigger
    {
        private const string TriggerTag = "Water";
        private const float WaistDepth = 1.35f; // 腰部
        private const float TorsoDepth = 1.4f; // 胸部
        private const float HeadDepth = 1.6f; // 头部
        /*
         * 游泳行为:
         * 1. 角色进入水域后，激活该脚本，脚本会监测角色的涉水深度
         * 2. 当角色的涉水深度到达脖子区域时，角色会进入游泳状态
         * 3. 如果角色从高空落下，则角色会沉入水中
         */

        private LocomotionAddOnManager m_LocomotionManager;
        private readonly LocomotionMode m_LocomotionMode = LocomotionMode.WaterLocomotion;
        private ThirdPersonLocomotion m_Locomotion;
        private GameObject m_TriggerObject;
        private Collider m_WaterCollider;

        [FormerlySerializedAs("m_IsStartSwimming")]
        [SerializeField, ReadOnly] private bool m_HasStartedSwimming;
        [SerializeField, ReadOnly] private bool m_IsSwimming;
        [SerializeField, ReadOnly] private bool m_IsUnderWater; // 表示角色在水下
        [SerializeField, ReadOnly] private bool m_InTheWater; // 表示角色已经进入水中，但是还没有达到游泳的条件
        [SerializeField, ReadOnly] private bool m_TriggerUnderWater;
        [SerializeField, ReadOnly] private bool m_HasInSwimmingState;
        [SerializeField, ReadOnly] private float m_WaterHeight;
        [SerializeField, ReadOnly] private float m_SubmersionDepth; // 涉水深度

        [SerializeField, BoxGroup("Swimming Info", showLabel: false), Range(5f, 8f)]
        private float m_WaterDrag = 3f;
        [SerializeField, BoxGroup("Swimming Info", showLabel: false), Range(12f, 18f)]
        private float m_Buoyancy = 12f;
        [SerializeField, BoxGroup("Swimming Info", showLabel: false)]
        private float m_WaterDepthOffset = 1.2f; // 角色涉水深度偏移量

        [SerializeField, BoxGroup("Water Collider Setting", showLabel: false)]
        private float m_CharacterColliderRadius = 0.5f;
        [SerializeField, BoxGroup("Water Collider Setting", showLabel: false)]
        private float m_CharacterColliderHeight = 1.5f;
        [SerializeField, BoxGroup("Water Collider Setting", showLabel: false)]
        private Vector3 m_CharacterColliderCenter = Vector3.zero;

        private float m_CurrentBuoyancy;

        public LocomotionMode LocomotionMode => m_LocomotionMode;
        public GameObject TriggerObject => m_TriggerObject;
        public string ObjectName => m_TriggerObject?.name ?? string.Empty;

        public event Action<LocomotionMode> LocomotionModeChanged;

        public void OnDestroy()
        {
            LocomotionModeChanged = null;

            if (!m_Locomotion) return;

            var characterTrigger = m_Locomotion.GetComponent<CharacterTriggerAndCollision>();

            if (characterTrigger)
            {
                characterTrigger.TriggerEntered -= TriggerEnter;
                characterTrigger.TriggerStayed -= TriggerStay;
                characterTrigger.TriggerExited -= TriggerExit;
            }
        }

        public void Initialize(LocomotionAddOnManager locomotionAddOnManager)
        {
            m_LocomotionManager = locomotionAddOnManager;
            m_Locomotion = locomotionAddOnManager.Locomotion;

            var characterTrigger = m_Locomotion.GetComponent<CharacterTriggerAndCollision>();

            if (characterTrigger)
            {
                characterTrigger.TriggerEntered += TriggerEnter;
                characterTrigger.TriggerStayed += TriggerStay;
                characterTrigger.TriggerExited += TriggerExit;
            }

            m_SubmersionDepth = 0f;
        }

        public void CutInLocomotion()
        {
            m_Locomotion.SetGravityDrag(m_WaterDrag);
        }

        public void CutOutLocomotion()
        {
            EndSwimming();
            m_Locomotion.SetCharacterColliderSize();
            m_Locomotion.SetGravityDrag();
            m_IsUnderWater = false;
            m_TriggerUnderWater = false;
            m_CurrentBuoyancy = 0f;
            m_SubmersionDepth = 0f;
        }

        public void LocomotionAddOnTick()
        {
            if (!m_InTheWater || !m_WaterCollider) return; // 如果角色不在水中，直接返回

            // 获取水面高度
            m_WaterHeight = m_WaterCollider.bounds.max.y;

            // 计算角色涉水深度
            m_SubmersionDepth = m_WaterHeight - CharacterCenter().y;

            // 计算当前涉水深度的浮力(越深浮力越大)
            m_CurrentBuoyancy = m_SubmersionDepth * m_Buoyancy;

            // 判断角色是否在水下
            m_IsUnderWater = m_SubmersionDepth > HeadDepth;

            /*
             * 1. 如果此时还未开始游泳，那么优先判断 TORSO 区域是否被水覆盖了(也就是角色的胸部是否被水覆盖了)
             * 2. 当 TORSO 区域被水覆盖了，说明开始游泳了，那么 m_IsSwimming 此时为 true
             * 3. 再次判断水是否高于腰部，那肯定是高于的，所以也会返回true
             * 4. 当角色慢慢走出水，走到水齐腰处时，m_IsSwimming 会返回 false 那么 m_IsSwimming 就是为 false
             */
            m_IsSwimming = m_IsSwimming ? m_SubmersionDepth > WaistDepth : m_SubmersionDepth > TorsoDepth;

            if (m_IsSwimming)
            {
                if (!m_HasStartedSwimming)
                {
                    StartSwimming(); // 内部会判断角色是否是从高空掉入水中，如果是从高空掉入水中，则会让角色沉入水中
                    m_HasInSwimmingState = true;
                }

                Up();
            }
            else
            {
                if (m_HasInSwimmingState)
                    EndSwimming();
            }

            if (m_IsUnderWater)
            {
                if (!m_TriggerUnderWater)
                {
                    m_TriggerUnderWater = true;
                    m_Locomotion.SetCharacterColliderSize(m_CharacterColliderHeight, m_CharacterColliderRadius, m_CharacterColliderCenter);
                }
            }
            else
            {
                if (m_TriggerUnderWater)
                {
                    m_TriggerUnderWater = false;
                    m_Locomotion.SetCharacterColliderSize();
                }
            }
        }

        public void TriggerEnter(Collider collider)
        {
            if (!collider.gameObject.CompareTag(TriggerTag)) return;

            m_TriggerObject = collider.gameObject;
            m_WaterCollider = collider.GetComponent<Collider>();
            m_InTheWater = true;
            m_LocomotionManager.SwitchLocomotion(LocomotionMode.WaterLocomotion);

        }

        public void TriggerStay(Collider collider)
        {
            if (!collider.gameObject.CompareTag(TriggerTag)) return;
        }

        public void TriggerExit(Collider collider)
        {
            if (!collider.gameObject.CompareTag(TriggerTag)) return;
            if (collider != m_WaterCollider) return;

            if (m_IsSwimming)
                EndSwimming();

            m_LocomotionManager.SwitchLocomotion(LocomotionMode.None);

            m_WaterHeight = float.MinValue;
            m_InTheWater = false;
            m_TriggerObject = null;
            m_WaterCollider = null;
            m_IsUnderWater = false;
            m_TriggerUnderWater = false;
            m_Locomotion.SetCharacterColliderSize();
        }

        private void Up()
        {
            if (m_SubmersionDepth <= TorsoDepth)
            {
                // 当角色上浮到胸部时，停止上浮，让角色处于水面处(禁用角色的垂直力就行)
                m_Locomotion.SetAdditionalVerticalAcceleration(0f);
                m_Locomotion.SetGravityEnabled(false);
                return;
            }

            m_Locomotion.SetAdditionalVerticalAcceleration(m_CurrentBuoyancy); // 施加浮力在角色的垂直方向上，使角色上浮
            m_Locomotion.SetGravityEnabled(true);
        }

        private void StartSwimming()
        {
            m_HasStartedSwimming = true;
            m_Locomotion.SetGroundCheckEnabled(false);
            LocomotionModeChanged?.Invoke(m_LocomotionMode);
        }

        private void EndSwimming()
        {
            m_HasStartedSwimming = false;
            m_IsSwimming = false;
            m_HasInSwimmingState = false;
            m_Locomotion.SetGroundCheckEnabled(true);
            m_Locomotion.SetGravityEnabled(true);
            m_Locomotion.SetAdditionalVerticalAcceleration(0f);
            LocomotionModeChanged?.Invoke(LocomotionMode.None);
        }

        private Vector3 CharacterCenter()
        {
            return m_Locomotion.Character.position +
                    Vector3.up * (m_Locomotion.GetCharacterColliderSize().height * 0.5f * m_WaterDepthOffset);
        }

        public void OnDrawGizmos()
        {
            if (!m_InTheWater) return;
            if (m_LocomotionManager == null) return;
            if (!m_Locomotion) return;

            var matrix = Gizmos.matrix; // 保存原始矩阵
            var position = new Vector3(CharacterCenter().x, m_WaterHeight, CharacterCenter().z); // 设置辅助图形的目标位置旋转和缩放
            Gizmos.color = Color.blue; // 设置颜色
            Gizmos.matrix = Matrix4x4.TRS(position, Quaternion.identity, new Vector3(1, 0.001f, 1)); // 控制辅助图形的矩阵让其平移到目标位置，没有旋转，缩放Y轴
            Gizmos.DrawWireSphere(Vector3.zero, .5f); // 由于上面我们设置了矩阵，所以这里的球形辅助图形会被平移到目标位置
            Gizmos.matrix = matrix; // 恢复矩阵，避免控制其它辅助图形

            Gizmos.color = Color.red; // 设置颜色
            Gizmos.matrix = Matrix4x4.TRS(CharacterCenter(), Quaternion.identity, new Vector3(1, 0.001f, 1)); // 控制辅助图形的矩阵让其平移到目标位置，没有旋转，缩放Y轴
            Gizmos.DrawWireSphere(Vector3.zero, .3f); // 由于上面我们设置了矩阵，所以这里的球形辅助图形会被平移到目标位置
            Gizmos.matrix = matrix; // 恢复矩阵，避免控制其它辅助图形
        }
    }
}
