using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [Serializable]
    public class SphereHitboxQuery : HitboxQuery
    {
        [SerializeField, LabelText("球心")] private Vector3 m_Center;
        [SerializeField, Min(0.0001f), LabelText("半径")] private float m_Radius = 0.5f;

        protected override Vector3 LocalCenter => m_Center;
        protected override float LocalRadius => SafeRadius(m_Radius);
        protected override float LocalSweepLength => LocalRadius * 2f;

        protected override int Overlap(Vector3 position, Quaternion rotation, Vector3 scale,
            Collider[] results, LayerMask layerMask)
        {
            return Physics.OverlapSphereNonAlloc(position, SafeRadius(LocalRadius * MaxComponent(scale)),
                results, layerMask, QueryTriggerInteraction.Ignore);
        }

        protected override Vector3 GetHitPoint(Collider target, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            return target.ClosestPoint(position);
        }

        protected override void DrawShape(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Gizmos.DrawWireSphere(position, SafeRadius(LocalRadius * MaxComponent(scale)));
        }
    }
}
