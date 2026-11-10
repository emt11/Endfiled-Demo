using System;
using System.Collections.Generic;

namespace GP
{
    /// <summary>按添加顺序保存属性修饰器，并维护修饰总和</summary>
    public class ValueAttributeModifierCollection
    {
        // 保留未裁剪的修饰总和，移除修饰器时才能准确撤销其贡献
        private long m_TotalValue;
        private readonly List<ValueAttributeModifier> m_Modifiers = new List<ValueAttributeModifier>();

        public int ClampedTotalValue => (int)Math.Max(int.MinValue, Math.Min(m_TotalValue, int.MaxValue));
        internal long TotalValue => m_TotalValue;

        public void AddModifier(ValueAttributeOperation operation, int amount)
        {
            var modifier = new ValueAttributeModifier(amount, operation);
            m_Modifiers.Add(modifier);
            m_TotalValue += modifier.SignedAmount;
        }

        /// <summary>按添加时的数量和方向，移除最近添加的一条匹配记录</summary>
        public bool RemoveModifier(ValueAttributeOperation operation, int amount)
        {
            if (amount < 0) return false;
            if (operation != ValueAttributeOperation.Add && operation != ValueAttributeOperation.Subtract) return false;

            // 倒序查找，每次只移除最近添加的一条数量和方向均匹配的记录
            for (int i = m_Modifiers.Count - 1; i >= 0; i--)
            {
                var modifier = m_Modifiers[i];
                if (!modifier.Matches(operation, amount)) continue;

                // 保留剩余记录的添加顺序，不能用末尾元素覆盖当前位置
                m_Modifiers.RemoveAt(i);
                m_TotalValue -= modifier.SignedAmount;

                return true;
            }

            return false;
        }
    }
}
