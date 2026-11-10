using System;
using System.Collections.Generic;
using UnityEngine;

namespace GP
{
    /// <summary>在前后两次姿态之间分段查询，降低快速挥动时的漏检概率。</summary>
    [Serializable]
    public abstract class HitboxQuery : IHitboxQuery
    {
        private const int InitialBufferSize = 20;
        private const int MaxSweepSegments = 32;
        private const float MinimumRadius = 0.0001f;

        private Collider[] m_ColliderBuffer = new Collider[InitialBufferSize];
        private readonly HashSet<GameObject> m_DetectedTargets = new HashSet<GameObject>();
        private readonly List<HitboxHit> m_Results = new List<HitboxHit>();
        private IReadOnlyList<HitboxHit> m_ResultsView;
        private Vector3 m_PreviousPosition;
        private Vector3 m_CurrentPosition;
        private Quaternion m_PreviousRotation;
        private Quaternion m_CurrentRotation;
        private Vector3 m_PreviousScale;
        private Vector3 m_CurrentScale;
        private int m_SweepSegments = 1;

        protected abstract Vector3 LocalCenter { get; }
        protected abstract float LocalRadius { get; }
        protected abstract float LocalSweepLength { get; }
        public bool IsDetecting { get; private set; }

        public void BeginDetection(Transform origin)
        {
            if (!origin) return;
            m_CurrentPosition = m_PreviousPosition = origin.TransformPoint(LocalCenter);
            m_CurrentRotation = m_PreviousRotation = origin.rotation;
            m_CurrentScale = m_PreviousScale = GetAbsoluteScale(origin.lossyScale);
            m_SweepSegments = 1;
            m_Results.Clear();
            IsDetecting = true;
        }

        public void EndDetection()
        {
            IsDetecting = false;
            m_Results.Clear();
            m_DetectedTargets.Clear();
        }

        public IReadOnlyList<HitboxHit> Detect(Transform origin, LayerMask layerMask)
        {
            m_ResultsView ??= m_Results.AsReadOnly();
            m_Results.Clear();
            m_DetectedTargets.Clear();
            if (!IsDetecting || !origin) return m_ResultsView;

            m_PreviousPosition = m_CurrentPosition;
            m_PreviousRotation = m_CurrentRotation;
            m_PreviousScale = m_CurrentScale;
            m_CurrentPosition = origin.TransformPoint(LocalCenter);
            m_CurrentRotation = origin.rotation;
            m_CurrentScale = GetAbsoluteScale(origin.lossyScale);

            var maxScale = Mathf.Max(MaxComponent(m_PreviousScale), MaxComponent(m_CurrentScale));
            var sampleDistance = Mathf.Max(MinimumRadius, LocalRadius * maxScale * 0.8f);
            var distance = Vector3.Distance(m_PreviousPosition, m_CurrentPosition);
            var arcLength = LocalSweepLength * maxScale
                * Quaternion.Angle(m_PreviousRotation, m_CurrentRotation) * Mathf.Deg2Rad;
            m_SweepSegments = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(distance, arcLength) / sampleDistance),
                1, MaxSweepSegments);

            for (var sampleIndex = 0; sampleIndex <= m_SweepSegments; sampleIndex++)
            {
                var progress = sampleIndex / (float)m_SweepSegments;
                var position = Vector3.Lerp(m_PreviousPosition, m_CurrentPosition, progress);
                var rotation = Quaternion.Slerp(m_PreviousRotation, m_CurrentRotation, progress);
                var scale = Vector3.Lerp(m_PreviousScale, m_CurrentScale, progress);
                int count;
                while (true)
                {
                    count = Overlap(position, rotation, scale, m_ColliderBuffer, layerMask);
                    if (count < m_ColliderBuffer.Length) break;
                    // 缓冲区满时扩容并重新查询，避免默默丢掉第 21 个及后续目标。
                    Array.Resize(ref m_ColliderBuffer, m_ColliderBuffer.Length * 2);
                }

                for (var hitIndex = 0; hitIndex < count; hitIndex++)
                {
                    var collider = m_ColliderBuffer[hitIndex];
                    if (!collider) continue;
                    var target = collider.GetComponentInParent<IDamageable>();
                    var targetObject = target?.DamageableObject;
                    if (!targetObject || !m_DetectedTargets.Add(targetObject)) continue;
                    m_Results.Add(new HitboxHit(target, GetHitPoint(collider, position, rotation, scale)));
                }
                Array.Clear(m_ColliderBuffer, 0, count);
            }
            return m_ResultsView;
        }

        protected static float MaxComponent(Vector3 value) => Mathf.Max(value.x, Mathf.Max(value.y, value.z));
        protected static float SafeRadius(float value) => Mathf.Max(MinimumRadius, value);

        private static Vector3 GetAbsoluteScale(Vector3 scale)
        {
            return new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
        }

        protected abstract int Overlap(Vector3 position, Quaternion rotation, Vector3 scale,
            Collider[] results, LayerMask layerMask);
        protected abstract Vector3 GetHitPoint(Collider target, Vector3 position, Quaternion rotation, Vector3 scale);
        protected abstract void DrawShape(Vector3 position, Quaternion rotation, Vector3 scale);

        public void DrawGizmos(Transform origin)
        {
            if (!origin) return;
            var previousColor = Gizmos.color;
            Gizmos.color = IsDetecting ? Color.red : Color.green;
            if (Application.isPlaying && IsDetecting)
            {
                for (var index = 0; index <= m_SweepSegments; index++)
                {
                    var progress = index / (float)m_SweepSegments;
                    DrawShape(Vector3.Lerp(m_PreviousPosition, m_CurrentPosition, progress),
                        Quaternion.Slerp(m_PreviousRotation, m_CurrentRotation, progress),
                        Vector3.Lerp(m_PreviousScale, m_CurrentScale, progress));
                }
            }
            else
            {
                DrawShape(origin.TransformPoint(LocalCenter), origin.rotation, GetAbsoluteScale(origin.lossyScale));
            }
            Gizmos.color = previousColor;
        }
    }
}
