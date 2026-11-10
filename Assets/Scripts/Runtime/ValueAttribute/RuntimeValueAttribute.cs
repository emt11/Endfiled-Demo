using System;

namespace GP
{
    public class RuntimeValueAttribute
    {
        private readonly ValueAttributeName m_AttributeName;
        private readonly int m_BaseValue;
        private readonly ValueAttributeModifierCalculator m_ModifierCalculator;
        private int m_CurrentValue;
        private int m_MaxValue;

        public ValueAttributeName AttributeName => m_AttributeName;
        public int BaseValue => m_BaseValue;
        public int CurrentValue => m_CurrentValue;
        public int MaxValue => m_MaxValue;

        public event Action<ValueAttributeName, int, int> ValueChanged;
        public event Action<ValueAttributeName> ValueReachedZero;

        public RuntimeValueAttribute(ValueAttributeAsset attributeAsset)
        {
            if (attributeAsset == null)
                throw new ArgumentNullException(nameof(attributeAsset));

            m_AttributeName = attributeAsset.AttributeName;
            m_BaseValue = Math.Max(0, attributeAsset.BaseValue);
            m_MaxValue = m_CurrentValue = m_BaseValue;
            m_ModifierCalculator = new ValueAttributeModifierCalculator();
        }

        public bool ModifyCurrentValue(ValueAttributeChangeInput input)
        {
            if (!IsValidInput(input)) return false;

            var previousCurrentValue = m_CurrentValue;
            // 先使用 long 运算，避免在限制范围之前发生 int 溢出。
            long updatedValue = input.Operation == ValueAttributeOperation.Add
                ? (long)m_CurrentValue + input.Amount
                : (long)m_CurrentValue - input.Amount;
            m_CurrentValue = (int)Math.Max(0L, Math.Min(updatedValue, m_MaxValue));
            return NotifyStateChanged(previousCurrentValue, m_MaxValue);
        }

        public void SetCurrentValueToZero()
        {
            var previousCurrentValue = m_CurrentValue;
            m_CurrentValue = 0;
            NotifyStateChanged(previousCurrentValue, m_MaxValue);
        }

        public void RestoreToMaxValue()
        {
            var previousCurrentValue = m_CurrentValue;
            var previousMaxValue = m_MaxValue;
            m_MaxValue = m_ModifierCalculator.CalculateMaxValue(m_BaseValue);
            m_CurrentValue = m_MaxValue;
            NotifyStateChanged(previousCurrentValue, previousMaxValue);
        }

        public bool AddModifier(ValueAttributeChangeInput input)
        {
            if (!IsValidInput(input) || input.Amount == 0) return false;

            m_ModifierCalculator.AddModifier(input.Operation, input.Amount);
            RecalculateMaxValue();
            return true;
        }

        public bool RemoveModifier(ValueAttributeChangeInput input)
        {
            if (!IsValidInput(input) || input.Amount == 0) return false;
            if (!m_ModifierCalculator.RemoveModifier(input.Operation, input.Amount)) return false;

            RecalculateMaxValue();
            return true;
        }

        public void NotifyCurrentState()
        {
            ValueChanged?.Invoke(m_AttributeName, m_CurrentValue, m_MaxValue);
        }

        private bool IsValidInput(ValueAttributeChangeInput input)
        {
            return input.AttributeName == m_AttributeName && input.IsValid;
        }

        private void RecalculateMaxValue()
        {
            var previousCurrentValue = m_CurrentValue;
            var previousMaxValue = m_MaxValue;
            m_MaxValue = m_ModifierCalculator.CalculateMaxValue(m_BaseValue);
            // 满值时跟随新上限，非满值时只裁剪超出上限的部分
            m_CurrentValue = previousCurrentValue == previousMaxValue ? m_MaxValue : Math.Min(previousCurrentValue, m_MaxValue);
            NotifyStateChanged(previousCurrentValue, previousMaxValue);
        }

        private bool NotifyStateChanged(int previousCurrentValue, int previousMaxValue)
        {
            if (previousCurrentValue == m_CurrentValue && previousMaxValue == m_MaxValue) return false;

            bool reachedZero = previousCurrentValue > 0 && m_CurrentValue == 0;
            ValueChanged?.Invoke(m_AttributeName, m_CurrentValue, m_MaxValue);

            // 变化回调可能恢复当前值，此时不再发送过期的归零通知。
            if (reachedZero && m_CurrentValue == 0)
                ValueReachedZero?.Invoke(m_AttributeName);

            return true;
        }
    }

    public readonly struct ValueAttributeChangeInput
    {
        public readonly ValueAttributeName AttributeName;
        public readonly int Amount;
        public readonly ValueAttributeOperation Operation;

        /// <summary>Amount 是非负数量，增减方向由 Operation 决定。</summary>
        public bool IsValid => Amount >= 0 && (Operation == ValueAttributeOperation.Add || Operation == ValueAttributeOperation.Subtract);

        public ValueAttributeChangeInput(ValueAttributeName attributeName, int amount, ValueAttributeOperation operation)
        {
            AttributeName = attributeName;
            Amount = amount;
            Operation = operation;
        }
    }

    public enum ValueAttributeOperation
    {
        Add,
        Subtract
    }
}
