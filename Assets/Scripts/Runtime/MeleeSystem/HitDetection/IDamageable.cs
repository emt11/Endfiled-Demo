using UnityEngine;

namespace GP
{
    public interface IDamageable
    {
        GameObject DamageableObject { get; }
        bool IsDead { get; }
        bool IsInvulnerable { get; }
        DamageResult TakeDamage(in DamageContext context);
    }

    public readonly struct DamageContext
    {
        public readonly int Amount;
        public readonly ValueAttributeOperation Operation;
        public readonly Vector3 HitPoint;
        public readonly Quaternion HitRotation;
        public readonly HitDirection Direction;
        public readonly Vector3 AttackerPosition;
        public readonly bool CanBeBlocked;
        public readonly StatusEffectAsset StatusEffect;

        public DamageContext(int amount, ValueAttributeOperation operation, Vector3 hitPoint,
            Quaternion hitRotation, HitDirection direction, Vector3 attackerPosition,
            bool canBeBlocked = true, StatusEffectAsset statusEffect = null)
        {
            Amount = amount;
            Operation = operation;
            HitPoint = hitPoint;
            HitRotation = hitRotation;
            Direction = direction;
            AttackerPosition = attackerPosition;
            CanBeBlocked = canBeBlocked;
            StatusEffect = statusEffect;
        }
    }

    public readonly struct DamageResult
    {
        public readonly bool WasApplied;
        public readonly bool IsDead;
        public readonly bool IsInvulnerable;
        public readonly DefensePosture Posture;
        public readonly int HealthDamage;
        public readonly int StaminaCost;

        public DamageResult(bool wasApplied, bool isDead, bool isInvulnerable,
            DefensePosture posture = DefensePosture.None, int healthDamage = 0, int staminaCost = 0)
        {
            WasApplied = wasApplied;
            IsDead = isDead;
            IsInvulnerable = isInvulnerable;
            Posture = posture;
            HealthDamage = healthDamage;
            StaminaCost = staminaCost;
        }
    }
}
