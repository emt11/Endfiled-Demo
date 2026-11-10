using UnityEngine;

namespace GP
{
    public interface IDamageable
    {
        GameObject DamageableObject { get; }
        void TakeDamage(in DamageContext context, AnimationAsset reactionAnimation);
    }

    public readonly struct DamageContext
    {
        public readonly int Amount;
        public readonly ValueAttributeOperation Operation;
        public readonly Vector3 HitPoint;
        public readonly Quaternion HitRotation;
        public readonly StatusEffectAsset StatusEffect;

        public DamageContext(int amount, ValueAttributeOperation operation, Vector3 hitPoint,
            Quaternion hitRotation, StatusEffectAsset statusEffect = null)
        {
            Amount = amount;
            Operation = operation;
            HitPoint = hitPoint;
            HitRotation = hitRotation;
            StatusEffect = statusEffect;
        }
    }
}
