using UnityEngine.Scripting.APIUpdating;
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [Serializable, LabelText("修改指定值属性操作")]
    [MovedFrom(true, sourceNamespace: "GP", sourceAssembly: "Assembly-CSharp", sourceClassName: "MondifyAttributeAction")]
    public class ModifyAttributeAction : IItemAction
    {
        [SerializeField, BoxGroup("Action Info", showLabel: false), LabelText("数值属性")]
        private ValueAttributeName m_ValueAttributeName;

        [SerializeField, BoxGroup("Action Info", showLabel: false), LabelText("数值属性操作符")]
        private ValueAttributeOperation m_ValueAttributeOperation;

        [SerializeField, BoxGroup("Action Info", showLabel: false), LabelText("数值属性值")]
        private int m_Value;

        [SerializeField, BoxGroup("Action Info", showLabel: false), LabelText("音效")]
        private AudioAsset m_ActionSFX;

        public ItemActionCategory ActionCategory => ItemActionCategory.Use;

        public bool CanExecute(RuntimeItem runtimeItem)
        {
            var bag = runtimeItem?.ItemInventory?.CharacterBag;
            return m_Value >= 0 && bag != null
                && (m_ValueAttributeOperation == ValueAttributeOperation.Add || m_ValueAttributeOperation == ValueAttributeOperation.Subtract)
                && bag.TryGetComponent<CharacterAttributeComponent>(out _);
        }

        public void Execute(RuntimeItem runtimeItem)
        {
            var bag = runtimeItem.ItemInventory.CharacterBag;
            var character = bag.GetComponent<CharacterAttributeComponent>();
            character.Initialize();
            character.CharacterAttribute.ModifyCurrentValue(new ValueAttributeChangeInput(
                m_ValueAttributeName, m_Value, m_ValueAttributeOperation));
            m_ActionSFX?.PlaySoundWithPool(bag.transform.position);
        }
    }
}
