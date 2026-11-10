using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [Serializable, LabelText("施加状态效果操作")]
    public class AddStatusEffectAction : IItemAction
    {
        [SerializeField, BoxGroup("Action Info", showLabel: false), LabelText("状态效果")]
        private StatusEffectAsset m_StatusEffect;

        public ItemActionCategory ActionCategory => ItemActionCategory.Use;

        public bool CanExecute(RuntimeItem runtimeItem)
        {
            var bag = runtimeItem?.ItemInventory?.CharacterBag;
            return m_StatusEffect != null && bag != null
                && bag.TryGetComponent<CharacterAttributeComponent>(out _);
        }

        public void Execute(RuntimeItem runtimeItem)
        {
            var character = runtimeItem.ItemInventory.CharacterBag.GetComponent<CharacterAttributeComponent>();
            character.Initialize();
            character.RuntimeStatusEffectManager.AddStatusEffect(m_StatusEffect);
        }

    }
}
