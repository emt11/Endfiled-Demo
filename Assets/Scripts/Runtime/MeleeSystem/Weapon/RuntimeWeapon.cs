using System;
using System.Collections.Generic;
using UnityEngine;

namespace GP
{
    /// <summary>持有某个角色创建的武器检测器；由装备它的战斗控制器负责释放</summary>
    public sealed class RuntimeWeapon : IDisposable
    {
        private readonly AnimationSource m_AnimationSource;
        private readonly IReadOnlyList<WeaponAttachmentBinding> m_AttachmentBindings;
        private readonly Dictionary<WeaponAttachmentPoint, Hitbox> m_Hitboxes =
            new Dictionary<WeaponAttachmentPoint, Hitbox>();
        private readonly List<Hitbox> m_ActiveHitboxes = new List<Hitbox>();
        private readonly IReadOnlyList<Hitbox> m_ActiveHitboxesView;
        private bool m_IsDisposed;

        public WeaponAsset Asset { get; }
        public bool IsEquipped { get; private set; }
        public IReadOnlyList<Hitbox> ActiveHitboxes => m_ActiveHitboxesView;

        public RuntimeWeapon(AnimationSource animationSource, WeaponAsset asset,
            IReadOnlyList<WeaponAttachmentBinding> attachmentBindings = null)
        {
            m_AnimationSource = animationSource ? animationSource : throw new ArgumentNullException(nameof(animationSource));
            Asset = asset ? asset : throw new ArgumentNullException(nameof(asset));
            m_AttachmentBindings = attachmentBindings ?? Array.Empty<WeaponAttachmentBinding>();
            m_ActiveHitboxesView = m_ActiveHitboxes.AsReadOnly();
        }

        public bool Equip()
        {
            if (m_IsDisposed || !m_AnimationSource || !Asset) return false;
            if (m_Hitboxes.Count == 0) CreateHitboxes();
            foreach (var hitbox in m_Hitboxes.Values)
                if (hitbox) hitbox.gameObject.SetActive(true);
            IsEquipped = true;
            return true;
        }

        public void Unequip()
        {
            DisableAllHitboxes();
            foreach (var hitbox in m_Hitboxes.Values)
                if (hitbox) hitbox.gameObject.SetActive(false);
            IsEquipped = false;
        }

        /// <summary>
        /// 开启或关闭指定挂载部位的命中检测
        /// 攻击判定窗口开始时开启，结束时关闭
        /// </summary>
        public void SetHitboxesEnabled(WeaponAttachmentPoint points, bool enabled)
        {
            if (m_IsDisposed || (enabled && !IsEquipped)) return;
            foreach (var entry in m_Hitboxes)
            {
                if ((points & entry.Key) == 0 || !entry.Value) continue;
                var hitbox = entry.Value;
                if (enabled)
                {
                    if (m_ActiveHitboxes.Contains(hitbox) || !hitbox.gameObject.activeInHierarchy) continue;
                    hitbox.enabled = true;
                    hitbox.BeginDetection();
                    m_ActiveHitboxes.Add(hitbox);
                }
                else
                {
                    hitbox.EndDetection();
                    hitbox.enabled = false;
                    m_ActiveHitboxes.Remove(hitbox);
                }
            }
        }

        public void DisableAllHitboxes()
        {
            foreach (var hitbox in m_Hitboxes.Values)
            {
                if (!hitbox) continue;
                hitbox.EndDetection();
                hitbox.enabled = false;
            }
            m_ActiveHitboxes.Clear();
        }

        public void Dispose()
        {
            if (m_IsDisposed) return;
            Unequip();
            m_IsDisposed = true;
            foreach (var hitbox in m_Hitboxes.Values)
            {
                if (!hitbox) continue;
                if (Application.isPlaying) UnityEngine.Object.Destroy(hitbox.gameObject);
                else UnityEngine.Object.DestroyImmediate(hitbox.gameObject);
            }
            m_Hitboxes.Clear();
        }

        private void CreateHitboxes()
        {
            var animator = m_AnimationSource.SourceAnimator;
            if (!animator)
            {
                Debug.LogWarning("武器需要已初始化的 AnimationSource 和 Animator。", m_AnimationSource);
                return;
            }

            foreach (var attachment in Asset.Attachments)
            {
                if (attachment == null || !attachment.TryGetBone(out _)) continue;
                if (m_Hitboxes.ContainsKey(attachment.AttachmentPoint))
                {
                    Debug.LogWarning($"武器挂载部位重复：{attachment.AttachmentPoint}", Asset);
                    continue;
                }

                var hitbox = attachment.CreateHitbox(animator, FindAttachmentTarget(attachment.AttachmentPoint));
                if (!hitbox) continue;
                hitbox.AttachmentPoint = attachment.AttachmentPoint;
                hitbox.EndDetection();
                hitbox.enabled = false;
                m_Hitboxes.Add(attachment.AttachmentPoint, hitbox);
            }
        }

        private Transform FindAttachmentTarget(WeaponAttachmentPoint point)
        {
            foreach (var binding in m_AttachmentBindings)
            {
                if (binding.AttachmentPoint == point) return binding.Target;
            }
            return null;
        }
    }
}
