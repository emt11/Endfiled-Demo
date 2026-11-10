using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    /// <summary>管理一个角色的运行时属性，由持有者显式初始化和释放。</summary>
    [Serializable]
    public class CharacterAttribute
    {
        [SerializeField, BoxGroup("Asset", showLabel: false), HideIf(nameof(m_Initialized))]
        private List<ValueAttributeAsset> m_ValueAttributeAssetList = new List<ValueAttributeAsset>();
        [NonSerialized]
        private bool m_Initialized;

        [NonSerialized]
        private Dictionary<ValueAttributeName, RuntimeValueAttribute> m_RuntimeValueAttributes = new Dictionary<ValueAttributeName, RuntimeValueAttribute>();

        public bool IsInitialized => m_Initialized;
        public int Count => m_RuntimeValueAttributes.Count;

        public event Action<ValueAttributeName, int, int> ValueChanged;
        public event Action<ValueAttributeName> ValueReachedZero;

        /// <summary>创建独立的运行时属性。重复调用不重置数值，也不重复订阅事件。</summary>
        public void Initialize()
        {
            if (m_Initialized) return;
            if (m_ValueAttributeAssetList == null)
                throw new InvalidOperationException("角色属性配置列表不能为 null。");

            // 先在临时字典中完成校验和创建，配置错误时不留下部分初始化的状态。
            var runtimeValueAttributes = new Dictionary<ValueAttributeName, RuntimeValueAttribute>();
            foreach (var asset in m_ValueAttributeAssetList)
            {
                if (asset == null)
                    throw new InvalidOperationException("角色属性配置列表中存在空资产。");

                if (runtimeValueAttributes.ContainsKey(asset.AttributeName))
                    throw new InvalidOperationException($"角色属性配置重复：{asset.AttributeName}。");

                runtimeValueAttributes.Add(asset.AttributeName, new RuntimeValueAttribute(asset));
            }

            m_RuntimeValueAttributes = runtimeValueAttributes;
            foreach (var attribute in m_RuntimeValueAttributes.Values)
            {
                attribute.ValueChanged += DoValueChange;
                attribute.ValueReachedZero += DoValueReachedZero;
            }

            m_Initialized = true;
        }

        /// <summary>解除内部事件订阅并丢弃运行时数据，保留配置和外部订阅。</summary>
        public void Deinitialize()
        {
            m_Initialized = false;
            foreach (var attribute in m_RuntimeValueAttributes.Values)
            {
                attribute.ValueChanged -= DoValueChange;
                attribute.ValueReachedZero -= DoValueReachedZero;
            }

            m_RuntimeValueAttributes.Clear();
        }

        public bool Contains(ValueAttributeName name)
        {
            return m_RuntimeValueAttributes.ContainsKey(name);
        }

        public bool TryGetValues(ValueAttributeName name, out int currentValue, out int maxValue)
        {
            currentValue = 0;
            maxValue = 0;
            if (!m_RuntimeValueAttributes.TryGetValue(name, out var attribute)) return false;

            currentValue = attribute.CurrentValue;
            maxValue = attribute.MaxValue;
            return true;
        }

        /// <summary>修改当前值；返回值表示数值是否实际发生变化。</summary>
        public bool ModifyCurrentValue(ValueAttributeChangeInput input)
        {
            EnsureInitialized();
            return m_RuntimeValueAttributes.TryGetValue(input.AttributeName, out var attribute)
                && attribute.ModifyCurrentValue(input);
        }

        /// <summary>添加上限修饰器；返回值表示是否成功添加记录。</summary>
        public bool AddModifier(ValueAttributeChangeInput input)
        {
            EnsureInitialized();
            return m_RuntimeValueAttributes.TryGetValue(input.AttributeName, out var attribute)
                && attribute.AddModifier(input);
        }

        /// <summary>使用添加时相同的数量和方向，移除一条匹配记录。</summary>
        public bool RemoveModifier(ValueAttributeChangeInput input)
        {
            EnsureInitialized();
            return m_RuntimeValueAttributes.TryGetValue(input.AttributeName, out var attribute)
                && attribute.RemoveModifier(input);
        }

        /// <summary>将当前值归零；已经为零或属性不存在时返回 false。</summary>
        public bool SetCurrentValueToZero(ValueAttributeName name)
        {
            EnsureInitialized();
            if (!m_RuntimeValueAttributes.TryGetValue(name, out var attribute)) return false;

            bool changed = attribute.CurrentValue != 0;
            attribute.SetCurrentValueToZero();
            return changed;
        }

        /// <summary>恢复到当前上限并保留修饰器；已经满值或属性不存在时返回 false。</summary>
        public bool RestoreToMaxValue(ValueAttributeName name)
        {
            EnsureInitialized();
            if (!m_RuntimeValueAttributes.TryGetValue(name, out var attribute)) return false;

            bool changed = attribute.CurrentValue != attribute.MaxValue;
            attribute.RestoreToMaxValue();
            return changed;
        }

        public void RestoreAllToMaxValue()
        {
            EnsureInitialized();
            // 回调可能释放并重新初始化容器，因此使用快照并跳过已经失效的实例。
            var attributes = new List<RuntimeValueAttribute>(m_RuntimeValueAttributes.Values);
            foreach (var attribute in attributes)
            {
                if (!IsCurrentAttribute(attribute)) continue;
                attribute.RestoreToMaxValue();
            }
        }

        /// <summary>显式广播所有属性的当前状态，供 UI 订阅后首次刷新。</summary>
        public void NotifyCurrentState()
        {
            EnsureInitialized();
            var attributes = new List<RuntimeValueAttribute>(m_RuntimeValueAttributes.Values);
            foreach (var attribute in attributes)
            {
                if (!IsCurrentAttribute(attribute)) continue;
                attribute.NotifyCurrentState();
            }
        }

        private void EnsureInitialized()
        {
            if (!m_Initialized)
                throw new InvalidOperationException("请先调用 CharacterAttribute.Initialize()。");
        }

        private bool IsCurrentAttribute(RuntimeValueAttribute attribute)
        {
            return m_Initialized
                && m_RuntimeValueAttributes.TryGetValue(attribute.AttributeName, out var currentAttribute)
                && ReferenceEquals(attribute, currentAttribute);
        }

        private void DoValueChange(ValueAttributeName valueAttributeName, int currentValue, int maxValue)
        {
            ValueChanged?.Invoke(valueAttributeName, currentValue, maxValue);
        }

        private void DoValueReachedZero(ValueAttributeName valueAttributeName)
        {
            ValueReachedZero?.Invoke(valueAttributeName);
        }
    }
}
