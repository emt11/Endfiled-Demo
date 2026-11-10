using System;

namespace GP
{
    /// <summary>管理属性修饰器，并根据基础值计算属性上限。</summary>
    public class ValueAttributeModifierCalculator
    {
        private readonly ValueAttributeModifierCollection m_Modifiers = new ValueAttributeModifierCollection();

        public int CalculateMaxValue(int baseValue)
        {
            long maxValue = baseValue + m_Modifiers.TotalValue;
            return (int)Math.Max(0L, Math.Min(maxValue, int.MaxValue));
        }

        public void AddModifier(ValueAttributeOperation operation, int amount)
        {
            m_Modifiers.AddModifier(operation, amount);
        }

        public bool RemoveModifier(ValueAttributeOperation operation, int amount)
        {
            return m_Modifiers.RemoveModifier(operation, amount);
        }
    }
}
