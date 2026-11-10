using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [Serializable]
    public class ItemTable
    {
        [SerializeField, BoxGroup("Item Info", showLabel: false), HorizontalGroup("Item Info/ItemTable"), LabelText("物品资产:")]
        private ItemAsset m_ItemAsset;
        [SerializeField, BoxGroup("Item Info", showLabel: false), HorizontalGroup("Item Info/ItemTable"), LabelText("物品数量:")]
        private int m_ItemCount;

        public ItemAsset ItemAsset => m_ItemAsset;
        public int ItemCount => m_ItemCount;
    }
}
