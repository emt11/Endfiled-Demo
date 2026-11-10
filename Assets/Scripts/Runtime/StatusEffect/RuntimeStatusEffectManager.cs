using System;
using System.Collections.Generic;

namespace GP
{
    public class RuntimeStatusEffectManager : IDisposable
    {
        // 资产身份决定是否为同一效果，避免名称哈希碰撞和同名资产互相覆盖
        private readonly Dictionary<StatusEffectAsset, RuntimeStatusEffect> m_RunningStatusEffects = new Dictionary<StatusEffectAsset, RuntimeStatusEffect>();
        private bool m_IsClearing;
        private bool m_IsDisposed;

        public CharacterAttribute CharacterAttribute { get; }
        public int Count => m_RunningStatusEffects.Count;

        public RuntimeStatusEffectManager(CharacterAttribute characterAttribute)
        {
            CharacterAttribute = characterAttribute ?? throw new ArgumentNullException(nameof(characterAttribute));
        }

        public bool TryGetStatusEffect(StatusEffectAsset asset, out RuntimeStatusEffect effect)
        {
            effect = null;
            return asset != null && m_RunningStatusEffects.TryGetValue(asset, out effect);
        }

        public bool AddStatusEffect(StatusEffectAsset asset, int stackCount = 1)
        {
            if (m_IsDisposed) throw new ObjectDisposedException(nameof(RuntimeStatusEffectManager));
            if (m_IsClearing || asset == null || stackCount <= 0) return false;
            if (!CharacterAttribute.IsInitialized)
                throw new InvalidOperationException("请先初始化目标角色属性。");

            if (m_RunningStatusEffects.TryGetValue(asset, out var existing))
            {
                if (!existing.IsCompleted) return existing.AddStacks(stackCount);
                OnEffectCompleted(existing);
            }

            var effect = new RuntimeStatusEffect(asset, CharacterAttribute, stackCount);
            m_RunningStatusEffects.Add(asset, effect);
            effect.Completed += OnEffectCompleted;
            try
            {
                effect.ApplyStatusEffect();
            }
            catch
            {
                effect.CancelStatusEffect();
                throw;
            }

            return true;
        }

        public bool RemoveStatusEffect(StatusEffectAsset asset, int? stackCount = null)
        {
            if (asset == null || !m_RunningStatusEffects.TryGetValue(asset, out var effect)) return false;
            if (stackCount.HasValue) return effect.RemoveStacks(stackCount.Value);

            effect.CancelStatusEffect();
            return true;
        }

        public bool RemoveStatusEffectByType(StatusEffectType type)
        {
            if (type == StatusEffectType.None) return false;

            var effects = new List<RuntimeStatusEffect>(m_RunningStatusEffects.Values);
            bool removed = false;
            foreach (var effect in effects)
            {
                if (effect.StatusEffectType != type || !IsCurrentEffect(effect)) continue;
                effect.CancelStatusEffect();
                removed = true;
            }

            return removed;
        }

        /// <summary>取消所有效果，保留管理器以供对象池下一次使用</summary>
        public void Clear()
        {
            if (m_IsClearing) return;
            m_IsClearing = true;
            try
            {
                var effects = new List<RuntimeStatusEffect>(m_RunningStatusEffects.Values);
                m_RunningStatusEffects.Clear();
                foreach (var effect in effects)
                {
                    effect.Completed -= OnEffectCompleted;
                    effect.CancelStatusEffect();
                }
            }
            finally
            {
                m_IsClearing = false;
            }
        }

        /// <summary>角色销毁时调用；清理后不允许再添加效果</summary>
        public void Dispose()
        {
            if (m_IsDisposed) return;
            m_IsDisposed = true;
            Clear();
        }

        private bool IsCurrentEffect(RuntimeStatusEffect effect)
        {
            return m_RunningStatusEffects.TryGetValue(effect.Asset, out var current)
                && ReferenceEquals(effect, current);
        }

        private void OnEffectCompleted(RuntimeStatusEffect effect)
        {
            effect.Completed -= OnEffectCompleted;
            if (IsCurrentEffect(effect)) m_RunningStatusEffects.Remove(effect.Asset);
        }
    }
}
