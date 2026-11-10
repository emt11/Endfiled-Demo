using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [Serializable]
    public class CapsuleHitboxQuery : HitboxQuery
    {

        [SerializeField, LabelText("中心")] private Vector3 m_Center;
        [SerializeField, Min(0.0001f), LabelText("半径")] private float m_Radius = 0.5f;
        [SerializeField, Min(0f), LabelText("两端球心距离")] private float m_Length = 1f;
        [SerializeField, LabelText("方向")] private CapsuleAxis m_Axis;
        [SerializeField, Min(0f), LabelText("正向长度系数")] private float m_PositiveExtent = 1f;
        [SerializeField, Min(0f), LabelText("反向长度系数")] private float m_NegativeExtent = 1f;

        protected override Vector3 LocalCenter => m_Center;
        protected override float LocalRadius => SafeRadius(m_Radius);
        protected override float LocalSweepLength => Mathf.Max(0f, m_Length)
            * Mathf.Max(Mathf.Max(0f, m_PositiveExtent), Mathf.Max(0f, m_NegativeExtent)) + LocalRadius * 2f;

        private void GetEndpoints(Vector3 position, Quaternion rotation, Vector3 scale,
            out Vector3 start, out Vector3 end)
        {
            var axis = m_Axis switch
            {
                CapsuleAxis.X => Vector3.right,
                CapsuleAxis.Y => Vector3.up,
                _ => Vector3.forward
            };
            var axisScale = m_Axis switch
            {
                CapsuleAxis.X => scale.x,
                CapsuleAxis.Y => scale.y,
                _ => scale.z
            };
            var offset = rotation * axis * (Mathf.Max(0f, m_Length) * axisScale * 0.5f);
            start = position + offset * Mathf.Max(0f, m_PositiveExtent);
            end = position - offset * Mathf.Max(0f, m_NegativeExtent);
        }

        protected override int Overlap(Vector3 position, Quaternion rotation, Vector3 scale,
            Collider[] results, LayerMask layerMask)
        {
            GetEndpoints(position, rotation, scale, out var start, out var end);
            return Physics.OverlapCapsuleNonAlloc(start, end, SafeRadius(LocalRadius * MaxComponent(scale)),
                results, layerMask, QueryTriggerInteraction.Ignore);
        }

        protected override Vector3 GetHitPoint(Collider target, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            GetEndpoints(position, rotation, scale, out var start, out var end);
            var direction = end - start;
            var lengthSquared = direction.sqrMagnitude;
            var progress = lengthSquared > 0f
                ? Mathf.Clamp01(Vector3.Dot(target.transform.position - start, direction) / lengthSquared) : 0f;
            return target.ClosestPoint(start + direction * progress);
        }

        protected override void DrawShape(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            GetEndpoints(position, rotation, scale, out var start, out var end);
            var radius = SafeRadius(LocalRadius * MaxComponent(scale));
            Gizmos.DrawWireSphere(start, radius);
            Gizmos.DrawWireSphere(end, radius);
            var direction = end - start;
            if (direction.sqrMagnitude <= 0f) return;
            direction.Normalize();
            var tangent = Vector3.Cross(direction,
                Mathf.Abs(Vector3.Dot(direction, Vector3.up)) < 0.9f ? Vector3.up : Vector3.right).normalized;
            var bitangent = Vector3.Cross(direction, tangent).normalized;
            Gizmos.DrawLine(start + tangent * radius, end + tangent * radius);
            Gizmos.DrawLine(start - tangent * radius, end - tangent * radius);
            Gizmos.DrawLine(start + bitangent * radius, end + bitangent * radius);
            Gizmos.DrawLine(start - bitangent * radius, end - bitangent * radius);
        }
        private enum CapsuleAxis { X, Y, Z }
    }
}
