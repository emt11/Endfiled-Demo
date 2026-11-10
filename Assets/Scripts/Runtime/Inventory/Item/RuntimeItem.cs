using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace GP
{
    public class RuntimeItem
    {
        private static int s_NextRuntimeId;
        private readonly ItemAsset m_ItemAsset;
        private readonly Dictionary<ItemActionCategory, List<IItemAction>> m_ItemActions;
        private readonly ItemInventory m_ItemInventory;

        private int m_ItemStack;
        private bool m_IsExecuting;

        public int RuntimeItemId { get; }
        public ItemAsset ItemAsset => m_ItemAsset;
        public int ItemStack => m_ItemStack;
        public int MaxStackSize => m_ItemAsset?.MaxStackSize ?? 1;
        public bool IsStackable => m_ItemAsset?.IsStackable ?? false;
        public bool IsEmpty => m_ItemStack == 0;
        public ItemCategory ItemCategory => m_ItemAsset.ItemCategory;

        public ItemInventory ItemInventory => m_ItemInventory;
        public EntityId ItemAssetId => m_ItemAsset.ItemAssetId;

        public RuntimeItem(ItemAsset itemAsset, ItemInventory itemInventory, int itemCount)
        {
            if (!itemAsset) throw new ArgumentNullException(nameof(itemAsset));
            if (itemCount <= 0) throw new ArgumentOutOfRangeException(nameof(itemCount));
            RuntimeItemId = CreateRuntimeId();
            m_ItemAsset = itemAsset;
            m_ItemInventory = itemInventory;
            m_ItemActions = new Dictionary<ItemActionCategory, List<IItemAction>>();
            m_ItemStack = m_ItemAsset.IsStackable ? Mathf.Min(itemCount, MaxStackSize) : 1;

            if (m_ItemAsset.ItemActions != null)
            {
                foreach (var action in m_ItemAsset.ItemActions)
                {
                    if (action == null) continue;
                    if (m_ItemActions.TryGetValue(action.ActionCategory, out var actions))
                    {
                        actions.Add(action);
                    }
                    else
                    {
                        m_ItemActions.Add(action.ActionCategory, new List<IItemAction> { action });
                    }
                }
            }
        }

        public bool AddItemStack(int stackCount = 1)
        {
            if (!m_ItemAsset || stackCount <= 0) return false;
            if (!m_ItemAsset.IsStackable) return false;
            if (m_ItemStack == MaxStackSize) return false;

            m_ItemStack = (int)Math.Min((long)m_ItemStack + stackCount, MaxStackSize);

            return true;
        }

        public bool RemoveItemStack(int stackCount = 1)
        {
            if (!m_ItemAsset || stackCount <= 0 || IsEmpty) return false;

            m_ItemStack = Mathf.Clamp(m_ItemStack - stackCount, 0, m_ItemAsset.MaxStackSize);

            return true;
        }

        public bool TryExecuteAction(ItemActionCategory actionCategory)
        {
            if (m_IsExecuting || IsEmpty || m_ItemInventory == null
                || !ReferenceEquals(m_ItemInventory.Peek(RuntimeItemId), this)
                || !m_ItemActions.TryGetValue(actionCategory, out var actions)) return false;

            m_IsExecuting = true;
            try
            {
                List<ItemTable> decomposition = null;
                foreach (var action in actions)
                {
                    if (action is DecomposeAction decompose)
                    {
                        decomposition ??= new List<ItemTable>();
                        decomposition.AddRange(decompose.RefundItems);
                    }
                    else if (!action.CanExecute(this)) return false;
                }

                // 同一物品的多个分解操作也属于同一批产物，不能逐个占用剩余空间
                if (decomposition != null && !m_ItemInventory.CharacterBag.TryAddItems(decomposition)) return false;
                foreach (var action in actions)
                    if (action is not DecomposeAction) action.Execute(this);
                if (actionCategory != ItemActionCategory.Equip)
                    m_ItemInventory.RemoveRuntimeItem(this);
                return true;
            }
            finally { m_IsExecuting = false; }
        }

        public bool HasItemAction(ItemActionCategory actionCategory)
        {
            return m_ItemActions.ContainsKey(actionCategory);
        }

        private static int CreateRuntimeId()
        {
            return Interlocked.Increment(ref s_NextRuntimeId);
        }
    }
}
