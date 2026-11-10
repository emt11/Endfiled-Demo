using UnityEngine;

namespace GP
{
    [DisallowMultipleComponent, RequireComponent(typeof(CharacterAttributeComponent))]
    public class DamageReceiver : MonoBehaviour, IDamageable
    {
        private AnimationSource m_AnimationSource;
        private CharacterAttributeComponent m_AttributeComponent;

        public GameObject DamageableObject => gameObject;
        public bool IsDead => m_AttributeComponent
            && m_AttributeComponent.CharacterAttribute.TryGetValues(ValueAttributeName.Health, out var health, out _)
            && health <= 0;

        private void Awake()
        {
            m_AnimationSource = GetComponentInChildren<AnimationSource>();
            m_AttributeComponent = GetComponent<CharacterAttributeComponent>();
            m_AttributeComponent.Initialize();
        }

        public void TakeDamage(in DamageContext context, AnimationAsset reactionAnimation)
        {
            if (!isActiveAndEnabled || !m_AttributeComponent || IsDead || context.Amount < 0) return;
            if (context.Operation != ValueAttributeOperation.Add && context.Operation != ValueAttributeOperation.Subtract) return;
            var attributes = m_AttributeComponent.CharacterAttribute;
            if (!attributes.IsInitialized || !attributes.Contains(ValueAttributeName.Health)) return;

            var changed = attributes.ModifyCurrentValue(
                new ValueAttributeChangeInput(ValueAttributeName.Health, context.Amount, context.Operation));
            if (changed && context.Operation == ValueAttributeOperation.Subtract && reactionAnimation && m_AnimationSource)
                m_AnimationSource.PlayAnimation(reactionAnimation);

            if (context.StatusEffect && !IsDead)
                m_AttributeComponent.RuntimeStatusEffectManager?.AddStatusEffect(context.StatusEffect);
        }
    }
}
