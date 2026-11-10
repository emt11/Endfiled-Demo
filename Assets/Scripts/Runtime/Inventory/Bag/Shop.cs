using UnityEngine.Scripting.APIUpdating;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [MovedFrom(true, sourceNamespace: "GP", sourceAssembly: "Assembly-CSharp", sourceClassName: "Shoper")]
    public class Shop : MonoBehaviour
    {
        [SerializeField]
        private CharacterBag m_TargetCharacterBag;

        [SerializeField]
        private List<ShopItem> m_Items = new List<ShopItem>();
        private bool m_IsPurchasing;

        private void Start()
        {
            foreach (var shopItem in m_Items)
            {
                shopItem?.Initialize(this);
            }
        }

        public bool BuyItem(ShopItem shopItem)
        {
            if (m_IsPurchasing || shopItem == null || !shopItem.ItemAsset || !shopItem.ItemPrice
                || shopItem.ItemCount <= 0 || shopItem.ItemPrice.CurrencyValue < 0 || !m_TargetCharacterBag) return false;

            m_IsPurchasing = true;
            try
            {
                var price = shopItem.ItemPrice;
                if (!m_TargetCharacterBag.TrySpendCurrency(price.CurrencyCategory, price.CurrencyValue))
                    return false;

                if (m_TargetCharacterBag.AddItem(shopItem.ItemAsset) == 0)
                {
                    m_TargetCharacterBag.GetCurrency(price.CurrencyCategory)?.AddCurrency(price.CurrencyValue);
                    return false;
                }

                shopItem.DecreaseStock();
                return true;
            }
            finally { m_IsPurchasing = false; }
        }

    }

    [Serializable]
    [MovedFrom(true, sourceNamespace: "GP", sourceAssembly: "Assembly-CSharp", sourceClassName: "DebugShopeItem")]
    public class ShopItem
    {
        [SerializeField, BoxGroup("Settings", showLabel: false), HorizontalGroup("Settings/Items"), LabelText("物品名称:")] private ItemAsset m_ItemAsset;
        [SerializeField, BoxGroup("Settings", showLabel: false), HorizontalGroup("Settings/Items"), LabelText("物品价格:")] private CurrencyAsset m_ItemPrice;
        [SerializeField, BoxGroup("Settings", showLabel: false), HorizontalGroup("Settings/Items"), LabelText("物品数量:")] private int m_ItemCount;
        [NonSerialized] private Shop m_Shop;

        public ItemAsset ItemAsset => m_ItemAsset;
        public CurrencyAsset ItemPrice => m_ItemPrice;
        public int ItemCount => m_ItemCount;

        public void Initialize(Shop shop)
        {
            m_Shop = shop;
        }

        internal void DecreaseStock() => m_ItemCount--;

        [Button("购买")]
        private void BuyItem()
        {
            if (m_Shop) m_Shop.BuyItem(this);
        }

    }

}
