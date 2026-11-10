using System;
using System.Collections.Generic;
using UnityEngine;

namespace GP
{
    public sealed class MeleeTargeting
    {
        private readonly Transform m_Character;
        private readonly float m_Radius;
        private readonly LayerMask m_LayerMask;
        private readonly List<IDamageable> m_Targets = new List<IDamageable>();
        private Collider[] m_Colliders = new Collider[16];

        public IDamageable FocusTarget { get; private set; }
        public event Action<IDamageable> FocusTargetChanged;

        public MeleeTargeting(Transform character, float radius, LayerMask layerMask)
        {
            m_Character = character;
            m_Radius = radius;
            m_LayerMask = layerMask;
        }

        public void FindTargets()
        {
            m_Targets.Clear();
            int count;
            do
            {
                count = Physics.OverlapSphereNonAlloc(m_Character.position, m_Radius, m_Colliders,
                    m_LayerMask, QueryTriggerInteraction.Collide);
                if (count < m_Colliders.Length) break;
                Array.Resize(ref m_Colliders, m_Colliders.Length * 2);
            } while (true);

            for (var i = 0; i < count; i++)
            {
                var target = m_Colliders[i].GetComponentInParent<IDamageable>();
                if (IsValid(target) && !m_Targets.Contains(target)) m_Targets.Add(target);
            }
            Array.Clear(m_Colliders, 0, count);
            // 固定切换顺序，避免物理查询返回顺序变化导致滚轮反复选中同一目标
            m_Targets.Sort(CompareTargets);
            if (!m_Targets.Contains(FocusTarget)) SetFocusTarget(ClosestTarget());
        }

        public void RefreshFocusTarget()
        {
            if (FocusTarget != null && !IsValid(FocusTarget)) FindTargets();
        }

        public void NextTarget() => ChangeTarget(1);
        public void PreviousTarget() => ChangeTarget(-1);

        public void ClearTarget()
        {
            m_Targets.Clear();
            SetFocusTarget(null);
        }

        private void ChangeTarget(int step)
        {
            FindTargets();
            if (m_Targets.Count == 0) return;
            var index = m_Targets.IndexOf(FocusTarget);
            SetFocusTarget(m_Targets[(index + step + m_Targets.Count) % m_Targets.Count]);
        }

        private void SetFocusTarget(IDamageable target)
        {
            if (ReferenceEquals(FocusTarget, target)) return;
            FocusTarget = target;
            FocusTargetChanged?.Invoke(target);
        }

        private bool IsValid(IDamageable target)
        {
            // 接口引用不会自动使用 Unity 的销毁判空语义
            if (target == null || (target is UnityEngine.Object unityObject && !unityObject)) return false;
            if (target is Behaviour behaviour && !behaviour.isActiveAndEnabled) return false;
            var targetObject = target.DamageableObject;
            return targetObject && targetObject.activeInHierarchy && !target.IsDead
                && targetObject != m_Character.gameObject && !targetObject.transform.IsChildOf(m_Character)
                && (targetObject.transform.position - m_Character.position).sqrMagnitude <= m_Radius * m_Radius;
        }

        private IDamageable ClosestTarget()
        {
            IDamageable closest = null;
            var closestDistance = float.PositiveInfinity;
            foreach (var target in m_Targets)
            {
                var distance = (target.DamageableObject.transform.position - m_Character.position).sqrMagnitude;
                if (distance >= closestDistance) continue;
                closestDistance = distance;
                closest = target;
            }
            return closest;
        }

        private static int CompareTargets(IDamageable left, IDamageable right)
        {
            return left.DamageableObject.GetEntityId().CompareTo(right.DamageableObject.GetEntityId());
        }
    }
}
