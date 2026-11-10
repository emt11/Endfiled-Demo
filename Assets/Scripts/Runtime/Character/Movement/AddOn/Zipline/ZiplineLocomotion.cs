using System;
using UnityEngine;

namespace GP
{
    [Serializable]
    public class ZiplineLocomotion : ILocomotionAddOn, ITrigger
    {
        private const string TriggerTag = "Zipline";
        private readonly LocomotionMode m_LocomotionMode = LocomotionMode.ZiplineLocomotion;
        private LocomotionAddOnManager m_LocomotionManager;
        private ThirdPersonLocomotion m_Locomotion;
        private GameObject m_TriggerObject;

        private Rope m_Rope;

        private bool m_UsingZipline;
        private bool m_IsDirectionReversed;

        private Vector3 MoveDirection => m_IsDirectionReversed ? -m_Rope.MoveDirection : m_Rope.MoveDirection;

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

        public void Initialize(LocomotionAddOnManager locomotionManager)
        {
            m_LocomotionManager = locomotionManager;
            m_Locomotion = locomotionManager.Locomotion;

            var characterTrigger = m_Locomotion.GetComponent<CharacterTriggerAndCollision>();

            if (characterTrigger)
            {
                characterTrigger.TriggerEntered += TriggerEnter;
                characterTrigger.TriggerStayed += TriggerStay;
                characterTrigger.TriggerExited += TriggerExit;
            }
        }

        public void CutInLocomotion()
        {
            m_Locomotion?.SetPhysicsDisabled(true);
            m_Locomotion?.SetInputFrozen(true);
            m_Locomotion?.SetGroundCheckEnabled(false);
            m_Locomotion?.SetGravityEnabled(false);

            StartZipline();
        }

        public void CutOutLocomotion()
        {
            m_Locomotion?.SetPhysicsDisabled(false);
            m_Locomotion?.SetInputFrozen(false);
            m_Locomotion?.SetGroundCheckEnabled(true);
            m_Locomotion?.SetGravityEnabled(true);

            m_UsingZipline = false;
            m_IsDirectionReversed = false;
            m_TriggerObject = null;
            m_Rope = null;
        }

        public void LocomotionAddOnTick()
        {
            ZiplineTick();
        }

        public void TriggerEnter(Collider collider)
        {
            if (!collider.gameObject.CompareTag(TriggerTag) || m_UsingZipline) return;

            m_TriggerObject = collider.gameObject;
            m_Rope = m_TriggerObject?.GetComponent<Rope>();
            if (!m_Rope || !m_Rope.ZiplinePoint) return;

            m_LocomotionManager.SwitchLocomotion(LocomotionMode.ZiplineLocomotion);
        }

        public void TriggerStay(Collider collider)
        {
            if (!collider.gameObject.CompareTag(TriggerTag)) return;
            if (collider.gameObject != m_TriggerObject) return;
        }

        public void TriggerExit(Collider collider)
        {
            if (!collider.gameObject.CompareTag(TriggerTag)) return;

            if (collider.gameObject != m_TriggerObject) return;

            m_TriggerObject = null;

            m_LocomotionManager.SwitchLocomotion(LocomotionMode.None);
            LocomotionModeChanged?.Invoke(LocomotionMode.None);
        }

        private void ZiplineTick()
        {
            if (!m_UsingZipline || !m_Rope) return;

            var position = m_Locomotion.Character.position;

            position += MoveDirection * (14f * Time.smoothDeltaTime);

            var direction = MoveDirection;

            direction.y = 0f;

            if (m_Rope.RopeDirection == Rope.RopeDirectionType.Horizontal)
                m_Locomotion.SetCharacterRotation(Quaternion.LookRotation(direction));

            m_Locomotion.SetCharacterPosition(position);
        }

        private void StartZipline()
        {
            if (m_UsingZipline) return;

            var characterPosition = m_Locomotion.Character.position; // 保存玩家当前位置

            m_Rope.ZiplinePoint.gameObject.SetActive(true);

            var targetHandPoint = m_Rope.GetPointOnRope(characterPosition); // 确定角色在绳索上的位置

            var currentHandPoint = m_Locomotion.Character.TransformPoint(m_Rope.ZiplinePoint.localPosition); // 计算手部辅助锚点的世界位置

            var positionOffset = targetHandPoint - currentHandPoint; // 绳索上的位置减去手部辅助锚点的世界位置就是角色最终要到达的位置

            characterPosition += positionOffset;

            /*
             * 如果方向未反转时，角色的朝向与目标方向的点积如果是整数，表示角色方向与目标方向一致。
             */

            m_Locomotion.SetCharacterPosition(characterPosition); // 应用位置

            var validateDirection = Vector3.Dot(m_Locomotion.Character.forward, m_Rope.MoveDirection);

            m_IsDirectionReversed = m_Rope.IsInverseDirection(targetHandPoint, validateDirection);

            m_UsingZipline = true;
            LocomotionModeChanged?.Invoke(m_LocomotionMode);
        }

        public void OnDrawGizmos() { }

    }
}
