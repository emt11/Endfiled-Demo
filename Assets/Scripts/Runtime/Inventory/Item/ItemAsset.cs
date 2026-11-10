using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace GP
{
    [CreateAssetMenu(fileName = "New ItemAsset", menuName = "GP/Inventory/Item/Item Asset")]
    public class ItemAsset : ScriptableObject
    {
        [SerializeField, BoxGroup("Item Info", showLabel: false), LabelText("物品名称:")]
        private string m_ItemName;
        [SerializeField, BoxGroup("Item Info", showLabel: false), LabelText("物品图标:")]
        [FormerlySerializedAs("m_Sprite")]
        private Sprite m_Icon;
        [SerializeField, BoxGroup("Item Info", showLabel: false), LabelText("物品类型:")]
        private ItemCategory m_ItemCategory;
        [Space, SerializeField, BoxGroup("Item Info", showLabel: false), TextArea, LabelText("物品描述:")]
        private string m_ItemDescription;

        [SerializeField, BoxGroup("Setting", showLabel: false), LabelText("是否可叠加:")]
        private bool m_IsStackable = true;
        [SerializeField, BoxGroup("Setting", showLabel: false), LabelText("最大叠加数量:"), ShowIf(nameof(m_IsStackable))]
        private int m_MaxStackSize = 1;

        [SerializeReference, BoxGroup("Item Set", showLabel: false), LabelText("物品操作行为")]
        private List<IItemAction> m_ItemActions = new List<IItemAction>();

        public Sprite Icon => m_Icon;
        public string ItemName => m_ItemName;
        public ItemCategory ItemCategory => m_ItemCategory;
        public string ItemDescription => m_ItemDescription;
        public bool IsStackable => m_IsStackable;
        public int MaxStackSize => Mathf.Max(1, m_MaxStackSize);

        public EntityId ItemAssetId => GetEntityId();
        public IReadOnlyList<IItemAction> ItemActions => m_ItemActions;
    }
}
