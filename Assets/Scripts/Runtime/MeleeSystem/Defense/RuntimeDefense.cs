using System;
using UnityEngine;

namespace GP
{
    public sealed class RuntimeDefense
    {
        private readonly CharacterAttribute m_Attributes;
        private readonly Transform m_Character;
        private float m_StartTime;
        private float m_ExitTime = float.NegativeInfinity;

        public DefenseAsset Asset { get; private set; }
        public bool IsDefending { get; private set; }

        public RuntimeDefense(CharacterAttribute attributes, Transform character)
        {
            m_Attributes = attributes;
            m_Character = character;
        }

        public void SetAsset(DefenseAsset asset)
        {
            if (asset && !m_Attributes.Contains(ValueAttributeName.Stamina))
                throw new InvalidOperationException($"{m_Character.name} 使用了防御配置，请在角色属性中添加 Stamina");
            End();
            Asset = asset;
        }

        public bool TryBegin()
        {
            if (IsDefending || !Asset || GetStamina() <= 0 || Time.time - m_ExitTime < Asset.Cooldown)
                return false;
            m_StartTime = Time.time;
            IsDefending = true;
            return true;
        }

        public void End()
        {
            if (!IsDefending) return;
            IsDefending = false;
            m_ExitTime = Time.time;
        }

        public DefenseResult Evaluate(in DamageContext context)
        {
            if (!IsDefending || !Asset || !context.CanBeBlocked || GetStamina() <= 0
                || context.Operation != ValueAttributeOperation.Subtract || !IsInFront(context.AttackerPosition))
                return new DefenseResult(DefensePosture.None, context.Amount);

            var elapsedTime = Time.time - m_StartTime;
            if (elapsedTime >= 0f && elapsedTime <= Asset.PerfectParryDuration)
                return new DefenseResult(DefensePosture.PerfectParry, 0);

            var staminaBefore = GetStamina();
            var staminaCost = Mathf.RoundToInt(context.Amount * Asset.StaminaCostMultiplier);
            m_Attributes.ModifyCurrentValue(new ValueAttributeChangeInput(
                ValueAttributeName.Stamina, staminaCost, ValueAttributeOperation.Subtract));
            var staminaAfter = GetStamina();
            var spent = Mathf.Max(0, staminaBefore - staminaAfter);
            if (staminaAfter > 0) return new DefenseResult(DefensePosture.Blocked, 0, spent);

            End();
            return new DefenseResult(DefensePosture.GuardBreak, context.Amount, spent);
        }

        private int GetStamina()
        {
            m_Attributes.TryGetValues(ValueAttributeName.Stamina, out var stamina, out _);
            return stamina;
        }

        private bool IsInFront(Vector3 attackerPosition)
        {
            var direction = Vector3.ProjectOnPlane(attackerPosition - m_Character.position, Vector3.up);
            if (direction.sqrMagnitude < 0.0001f) return true;
            var forward = Vector3.ProjectOnPlane(m_Character.forward, Vector3.up).normalized;
            return Vector3.Dot(forward, direction.normalized) >= Mathf.Cos(Asset.BlockAngle * 0.5f * Mathf.Deg2Rad);
        }
    }
}
