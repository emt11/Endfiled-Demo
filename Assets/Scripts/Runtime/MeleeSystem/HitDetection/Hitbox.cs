using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    public class Hitbox : MonoBehaviour
    {
        [SerializeField, EnumToggleButtons, LabelText("形状")]
        private HitboxShape m_Shape;
        [SerializeField, ShowIf(nameof(m_Shape), HitboxShape.Capsule), LabelText("胶囊")]
        private CapsuleHitboxQuery m_CapsuleQuery = new CapsuleHitboxQuery();
        [SerializeField, ShowIf(nameof(m_Shape), HitboxShape.Sphere), LabelText("球体")]
        private SphereHitboxQuery m_SphereQuery = new SphereHitboxQuery();
        [SerializeField, LabelText("目标层")] private LayerMask m_LayerMask;

        private IHitboxQuery m_ActiveQuery;
        public WeaponAttachmentPoint AttachmentPoint { get; internal set; }
        public bool IsDetecting => m_ActiveQuery?.IsDetecting ?? false;

        private void OnDisable()
        {
            EndDetection();
        }

        public void BeginDetection()
        {
            if (IsDetecting) return;
            m_ActiveQuery = m_Shape == HitboxShape.Capsule ? m_CapsuleQuery : m_SphereQuery;
            m_ActiveQuery?.BeginDetection(transform);
        }

        public void EndDetection()
        {
            m_ActiveQuery?.EndDetection();
        }

        public IReadOnlyList<HitboxHit> Detect()
        {
            if (!isActiveAndEnabled || m_ActiveQuery == null) return Array.Empty<HitboxHit>();
            return m_ActiveQuery.Detect(transform, m_LayerMask);
        }

        private void OnDrawGizmos()
        {
            IHitboxQuery query = m_Shape == HitboxShape.Capsule ? m_CapsuleQuery : m_SphereQuery;
            query?.DrawGizmos(transform);
        }
    }
}
