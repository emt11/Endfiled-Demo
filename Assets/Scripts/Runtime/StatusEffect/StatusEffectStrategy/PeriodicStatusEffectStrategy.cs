using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    /// <summary>每次触发修改一次当前属性值，不保存角色状态，也不自行启动循环</summary>
    [Serializable]
    public class PeriodicStatusEffectStrategy : IStatusEffectStrategy
    {
        [SerializeField, LabelText("目标属性")]
        private ValueAttributeName m_EffectValueName = ValueAttributeName.Health;
        [SerializeField, LabelText("数值运算")]
        private ValueAttributeOperation m_EffectValueOperation = ValueAttributeOperation.Subtract;

        public void OnApplyEffect(RuntimeStatusEffect effect)
        {
        }

        public void TickEffect(RuntimeStatusEffect effect)
        {
            if (effect == null) throw new ArgumentNullException(nameof(effect));
            if (!effect.IsRunning) return;

            effect.TargetAttributes.ModifyCurrentValue(new ValueAttributeChangeInput(
                m_EffectValueName, effect.EffectValue, m_EffectValueOperation));
        }

        public void OnRemoveEffect(RuntimeStatusEffect effect)
        {
            // 已经产生的伤害或治疗不会在移除状态时撤销
        }
    }
}
