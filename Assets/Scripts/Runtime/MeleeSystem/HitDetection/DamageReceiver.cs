using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [DisallowMultipleComponent, RequireComponent(typeof(CharacterAttributeComponent))]
    public class DamageReceiver : MonoBehaviour, IDamageable
    {
        [SerializeField, LabelText("未装备武器时的受击反应")]
        private DamageReactionAsset m_DefaultReactions;
        [SerializeField, LabelText("未装备武器时的防御配置")]
        private DefenseAsset m_DefaultDefense;
        [SerializeField, LabelText("死亡动画")]
        private AnimationAsset m_DeathAnimation;
        [SerializeField, LabelText("死亡音效")]
        private AudioAsset m_DeathSound;
        [SerializeField, LabelText("死亡时禁用的碰撞体")]
        private Collider[] m_DeathColliders = Array.Empty<Collider>();

        private AnimationSource m_AnimationSource;
        private CharacterAttributeComponent m_AttributeComponent;
        private MeleeCombatController m_CombatController;
        private DamageReactionAsset m_Reactions;
        private bool m_DeathHandled;

        public GameObject DamageableObject => gameObject;
        public bool IsInvulnerable { get; private set; }
        public bool HasHyperArmor { get; private set; }
        public bool IsDead => m_AttributeComponent
            && m_AttributeComponent.CharacterAttribute.TryGetValues(ValueAttributeName.Health, out var health, out _)
            && health <= 0;
        public RuntimeDefense Defense { get; private set; }

        private void Awake()
        {
            m_AnimationSource = GetComponentInChildren<AnimationSource>();
            m_CombatController = GetComponent<MeleeCombatController>();
            m_AttributeComponent = GetComponent<CharacterAttributeComponent>();
            m_AttributeComponent.Initialize();
            var attributes = m_AttributeComponent.CharacterAttribute;
            if (!attributes.Contains(ValueAttributeName.Health))
                throw new InvalidOperationException($"{name} 的 DamageReceiver 需要 Health 属性，请在 CharacterAttributeComponent 中配置");
            Defense = new RuntimeDefense(attributes, transform);
        }

        private void OnEnable()
        {
            m_AttributeComponent.CharacterAttribute.ValueReachedZero += HandleValueReachedZero;
            if (m_CombatController) m_CombatController.WeaponChanged += HandleWeaponChanged;
            HandleWeaponChanged(m_CombatController ? m_CombatController.CurrentWeapon : null);
            if (IsDead) HandleDeath();
        }

        private void OnDisable()
        {
            m_AttributeComponent.CharacterAttribute.ValueReachedZero -= HandleValueReachedZero;
            if (m_CombatController) m_CombatController.WeaponChanged -= HandleWeaponChanged;
            Defense.End();
            EventCenter<TargetUnavailableEvent>.Publish(new TargetUnavailableEvent(this));
        }

        public void SetInvulnerable(bool invulnerable) => IsInvulnerable = invulnerable;
        public void SetHyperArmor(bool hyperArmor) => HasHyperArmor = hyperArmor;

        public bool TryStartDefense()
        {
            if (!isActiveAndEnabled || IsDead) return false;
            if (m_CombatController && m_CombatController.CombatStatus == CombatStatus.Action) return false;
            if (!Defense.TryBegin()) return false;
            if (m_CombatController) m_CombatController.CancelAttack();
            return true;
        }

        public void EndDefense() => Defense.End();

        public DamageResult TakeDamage(in DamageContext context)
        {
            if (!isActiveAndEnabled || IsDead || context.Amount < 0
                || (context.Operation != ValueAttributeOperation.Add && context.Operation != ValueAttributeOperation.Subtract)
                || (IsInvulnerable && context.Operation == ValueAttributeOperation.Subtract))
            {
                var rejectedResult = new DamageResult(false, IsDead, IsInvulnerable);
                LogRejectedDamage(context, rejectedResult);
                return rejectedResult;
            }

            var attributes = m_AttributeComponent.CharacterAttribute;
            var isDamage = context.Operation == ValueAttributeOperation.Subtract;
            var defense = isDamage && !HasHyperArmor
                ? Defense.Evaluate(context) : new DefenseResult(DefensePosture.None, context.Amount);
            attributes.TryGetValues(ValueAttributeName.Health, out var previousHealth, out _);
            attributes.ModifyCurrentValue(new ValueAttributeChangeInput(
                ValueAttributeName.Health, defense.HealthDamage, context.Operation));

            if (isActiveAndEnabled && !IsDead && context.StatusEffect && defense.HealthDamage > 0)
                m_AttributeComponent.RuntimeStatusEffectManager.AddStatusEffect(context.StatusEffect);
            attributes.TryGetValues(ValueAttributeName.Health, out var currentHealth, out _);

            if (isActiveAndEnabled && !IsDead && isDamage && !HasHyperArmor && context.Amount > 0)
                PlayDamageReaction(context, defense.Posture);

            // 属性和状态效果回调可能当场致死，结果必须取处理完成后的状态
            var result = new DamageResult(true, IsDead, IsInvulnerable, defense.Posture,
                isDamage ? Mathf.Max(0, previousHealth - currentHealth) : 0, defense.StaminaCost);
            LogDamageResult(context, defense, result, previousHealth, currentHealth);
            return result;
        }

        private void PlayDamageReaction(in DamageContext context, DefensePosture posture)
        {
            var defenseAction = Defense.Asset ? Defense.Asset.GetAction(posture) : null;
            DamageReactionResult reaction;
            if (posture == DefensePosture.None)
                reaction = m_Reactions ? m_Reactions.FindReactionResult(context.Direction, context.Amount) : default;
            else
                reaction = defenseAction?.FindReaction(context.Direction, context.Amount) ?? default;

            if (m_CombatController)
            {
                m_CombatController.CancelAttack();
                m_CombatController.PlayReaction(reaction);
            }
            else if (reaction.IsValid && m_AnimationSource)
            {
                m_AnimationSource.PlayAnimation(reaction.Animation);
            }
            defenseAction?.PlayFeedback(context.HitPoint, context.HitRotation);
        }

        private void LogRejectedDamage(in DamageContext context, in DamageResult result)
        {
            var reason = !isActiveAndEnabled ? "组件未启用"
                : result.IsDead ? "目标已死亡"
                : context.Amount < 0 ? "输入值为负数"
                : context.Operation != ValueAttributeOperation.Add && context.Operation != ValueAttributeOperation.Subtract
                    ? "不支持的属性操作" : "目标处于无敌状态";
            Debug.Log(
                $"[TakeDamage] 目标={name}，帧={Time.frameCount}，未结算：{reason}，操作={context.Operation}，输入值={context.Amount}",
                this);
        }

        private void LogDamageResult(in DamageContext context, in DefenseResult defense, in DamageResult result,
            int previousHealth, int currentHealth)
        {
            Debug.Log(
                $"[TakeDamage] 目标={name}，帧={Time.frameCount}，操作={context.Operation}，输入值={context.Amount}，"
                + $"防御后数值={defense.HealthDamage}，实际扣血={result.HealthDamage}，生命={previousHealth} -> {currentHealth}，"
                + $"防御={result.Posture}，体力消耗={result.StaminaCost}，死亡={result.IsDead}",
                this);
        }

        private void HandleWeaponChanged(RuntimeWeapon weapon)
        {
            var asset = weapon?.Asset;
            Defense.SetAsset(asset ? asset.Defense : m_DefaultDefense);
            m_Reactions = asset && asset.DamageReactions ? asset.DamageReactions : m_DefaultReactions;
        }

        private void HandleValueReachedZero(ValueAttributeName attribute)
        {
            if (attribute == ValueAttributeName.Health && IsDead) HandleDeath();
            if (attribute == ValueAttributeName.Stamina) Defense.End();
        }

        private void HandleDeath()
        {
            if (m_DeathHandled) return;
            m_DeathHandled = true;
            Defense.End();
            if (m_CombatController)
            {
                m_CombatController.CancelAttack();
                m_CombatController.SetActionActive(false);
                m_CombatController.SetActionActive(true);
            }
            m_AttributeComponent.RuntimeStatusEffectManager.Clear();
            var locomotion = GetComponent<ThirdPersonLocomotion>();
            if (locomotion) locomotion.enabled = false;
            foreach (var collider in m_DeathColliders)
                if (collider) collider.enabled = false;
            if (m_DeathAnimation && m_AnimationSource) m_AnimationSource.PlayAnimation(m_DeathAnimation);
            if (m_DeathSound) m_DeathSound.PlaySoundWithPool(transform.position);
            EventCenter<TargetUnavailableEvent>.Publish(new TargetUnavailableEvent(this));
        }
    }
}
