using System;

namespace GP
{
    /// <summary>一条不可变的固定数值修饰，保存数量和加减方向</summary>
    public readonly struct ValueAttributeModifier
    {
        public readonly int Amount;
        public readonly ValueAttributeOperation Operation;

        internal long SignedAmount => Operation == ValueAttributeOperation.Add ? Amount : -(long)Amount;

        public ValueAttributeModifier(int amount, ValueAttributeOperation operation)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "修饰值不能为负数。");
            if (operation != ValueAttributeOperation.Add && operation != ValueAttributeOperation.Subtract)
                throw new ArgumentOutOfRangeException(nameof(operation), operation, "不支持的属性运算。");

            Amount = amount;
            Operation = operation;
        }

        public bool Matches(int amount)
        {
            return Amount == amount;
        }

        public bool Matches(ValueAttributeOperation operation, int amount)
        {
            return Operation == operation && Amount == amount;
        }
    }
}
