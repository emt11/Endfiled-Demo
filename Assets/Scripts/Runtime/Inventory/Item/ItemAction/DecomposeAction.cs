using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [Serializable, LabelText("分解物品操作")]
    public class DecomposeAction : IItemAction
    {
        [Space, SerializeField, BoxGroup("Action Info", showLabel: false), LabelText("返还物品:")]
        private List<ItemTable> m_RefundItems = new List<ItemTable>();

        [Space, SerializeField, BoxGroup("Action Info", showLabel: false), LabelText("音效:")]
        private AudioAsset m_ActionAudioAsset;

        internal IReadOnlyList<ItemTable> RefundItems => m_RefundItems;

        public ItemActionCategory ActionCategory => ItemActionCategory.Decompose;

        public bool CanExecute(RuntimeItem runtimeItem)
        {
            return runtimeItem?.ItemInventory?.CharacterBag != null
                && runtimeItem.ItemInventory.CharacterBag.CanAddItems(m_RefundItems);
        }

        public void Execute(RuntimeItem runtimeItem)
        {
            if (!runtimeItem.ItemInventory.CharacterBag.TryAddItems(m_RefundItems))
                throw new InvalidOperationException("分解产物空间不足；必须在执行前通过 CanExecute 检查。");
        }
    }
}
