using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [Serializable, LabelText("出售操作")]
    public class SellAction : IItemAction
    {
        [SerializeField, BoxGroup("Action Info", showLabel: false), HorizontalGroup("Action Info/Sell"), LabelText("出售价格:")]
        private CurrencyAsset m_SellPrice;

        [SerializeField, BoxGroup("Action Info", showLabel: false), HorizontalGroup("Action Info/Sell"), LabelText("操作音效:")]
        private AudioAsset m_ActionSFX;

        public ItemActionCategory ActionCategory => ItemActionCategory.Sell; // 方便UI根据操作名称显示操作按钮

        public bool CanExecute(RuntimeItem runtimeItem)
        {
            return runtimeItem?.ItemInventory?.CharacterBag != null && m_SellPrice
                && m_SellPrice.CurrencyCategory != CurrencyCategory.None && m_SellPrice.CurrencyValue > 0;
        }

        public void Execute(RuntimeItem runtimeItem)
        {
            var bag = runtimeItem.ItemInventory.CharacterBag;
            bag.AddCurrency(m_SellPrice);
            m_ActionSFX?.PlaySoundWithPool(bag.transform.position);
        }
    }
}
