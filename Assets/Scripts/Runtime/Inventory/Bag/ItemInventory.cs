using System;
using System.Collections.Generic;
using UnityEngine;

namespace GP
{
    public class ItemInventory : IDisposable
    {
        internal const int DefaultCapacity = 20;

        private readonly Dictionary<int, RuntimeItem> m_ActiveItems = new();
        private readonly Dictionary<EntityId, RuntimeItem> m_StackedItems = new();
        private readonly CharacterBag m_CharacterBag;
        private readonly ItemCategory m_InventoryCategory;
        private int m_InventoryMaxCapacity = DefaultCapacity;

        internal int AvailableSlots => Mathf.Max(0, m_InventoryMaxCapacity - m_ActiveItems.Count);

        public CharacterBag CharacterBag => m_CharacterBag;
        public int InventoryMaxCapacity => m_InventoryMaxCapacity;
        public bool IsInventoryFull => m_ActiveItems.Count >= m_InventoryMaxCapacity;

        /// <summary>物品及本次实际加入的数量</summary>
        public event Action<RuntimeItem, int> ItemAdded;
        /// <summary>物品及移除后的剩余数量</summary>
        public event Action<RuntimeItem, int> ItemRemoved;

        public ItemInventory(CharacterBag bag, ItemCategory inventoryCategory)
        {
            m_CharacterBag = bag;
            m_InventoryCategory = inventoryCategory;
        }

        public int GetAddableCount(ItemAsset asset, int count)
        {
            if (!asset || count <= 0 || asset.ItemCategory != m_InventoryCategory) return 0;
            if (asset.IsStackable)
            {
                if (m_StackedItems.TryGetValue(asset.ItemAssetId, out var stack))
                    return Mathf.Min(count, stack.MaxStackSize - stack.ItemStack);
                return IsInventoryFull ? 0 : Mathf.Min(count, asset.MaxStackSize);
            }

            return Mathf.Min(count, Mathf.Max(0, m_InventoryMaxCapacity - m_ActiveItems.Count));
        }

        public int AddRuntimeItem(RuntimeItem item)
        {
            int added = AddItemCore(item, out var storedItem);
            if (added > 0) ItemAdded?.Invoke(storedItem, added);
            return added;
        }

        public int AddRuntimeItem(ItemAsset asset, int count = 1)
        {
            int addable = GetAddableCount(asset, count);
            if (addable == 0) return 0;
            if (asset.IsStackable)
                return AddRuntimeItem(new RuntimeItem(asset, this, addable));

            int added = 0;
            for (int i = 0; i < addable; i++)
                added += AddRuntimeItem(new RuntimeItem(asset, this, 1));
            return added;
        }

        public int RemoveRuntimeItem(int id, int count = 1)
        {
            if (count <= 0 || !m_ActiveItems.TryGetValue(id, out var item)) return 0;
            int removed = RemoveItemCore(item, count);
            if (removed > 0) ItemRemoved?.Invoke(item, item.ItemStack);
            return removed;
        }

        public int RemoveRuntimeItem(RuntimeItem item, int count = 1)
        {
            return item == null ? 0 : RemoveRuntimeItem(item.RuntimeItemId, count);
        }

        public RuntimeItem Peek(int id)
        {
            return m_ActiveItems.GetValueOrDefault(id);
        }

        public RuntimeItem Pop(int id)
        {
            if (!m_ActiveItems.TryGetValue(id, out var item)) return null;
            m_ActiveItems.Remove(id);
            if (item.IsStackable) m_StackedItems.Remove(item.ItemAssetId);
            ItemRemoved?.Invoke(item, 0);
            return item;
        }

        public void SetInventoryMaxCapacity(int maxCapacity)
        {
            if (maxCapacity > m_InventoryMaxCapacity) m_InventoryMaxCapacity = maxCapacity;
        }

        public IReadOnlyList<RuntimeItem> GetActiveItems()
        {
            return new List<RuntimeItem>(m_ActiveItems.Values).AsReadOnly();
        }

        public void Dispose()
        {
            ItemAdded = null;
            ItemRemoved = null;
            m_ActiveItems.Clear();
            m_StackedItems.Clear();
        }

        internal int GetRequiredSlots(ItemAsset asset, int count)
        {
            return asset.IsStackable ? (m_StackedItems.ContainsKey(asset.ItemAssetId) ? 0 : 1) : count;
        }

        internal void AddBatchItem(ItemAsset asset, int count,
            List<(ItemInventory inventory, RuntimeItem item, int count)> addedItems)
        {
            int units = asset.IsStackable ? 1 : count;
            int stackCount = asset.IsStackable ? count : 1;
            for (int i = 0; i < units; i++)
            {
                int added = AddItemCore(new RuntimeItem(asset, this, stackCount), out var stored);
                addedItems.Add((this, stored, added));
            }
        }

        internal void NotifyItemAdded(RuntimeItem item, int count)
        {
            ItemAdded?.Invoke(item, count);
        }

        private int AddItemCore(RuntimeItem item, out RuntimeItem storedItem)
        {
            storedItem = null;
            if (item == null || m_ActiveItems.ContainsKey(item.RuntimeItemId)) return 0;
            int added = GetAddableCount(item.ItemAsset, item.ItemStack);
            if (added == 0) return 0;

            if (item.IsStackable && m_StackedItems.TryGetValue(item.ItemAssetId, out storedItem))
            {
                storedItem.AddItemStack(added);
                return added;
            }

            // 跨背包加入时创建属于新背包的实例，物品行为不会继续访问旧角色
            storedItem = ReferenceEquals(item.ItemInventory, this) && added == item.ItemStack
                ? item : new RuntimeItem(item.ItemAsset, this, added);
            m_ActiveItems.Add(storedItem.RuntimeItemId, storedItem);
            if (storedItem.IsStackable) m_StackedItems.Add(storedItem.ItemAssetId, storedItem);
            return added;
        }

        private int RemoveItemCore(RuntimeItem item, int count)
        {
            int removed = Mathf.Min(item.ItemStack, count);
            item.RemoveItemStack(removed);
            if (item.IsEmpty)
            {
                m_ActiveItems.Remove(item.RuntimeItemId);
                if (item.IsStackable) m_StackedItems.Remove(item.ItemAssetId);
            }
            return removed;
        }

        internal bool TransferItemTo(ItemInventory target, RuntimeItem item, int count)
        {
            if (target == null || ReferenceEquals(target, this) || item == null || count <= 0
                || !ReferenceEquals(Peek(item.RuntimeItemId), item) || count > item.ItemStack
                || target.GetAddableCount(item.ItemAsset, count) != count) return false;

            var transferredItem = new RuntimeItem(item.ItemAsset, target, count);
            int added = target.AddItemCore(transferredItem, out var storedItem);
            if (added != count) return false;
            RemoveItemCore(item, count);

            // 两边的数据全部提交后再通知，回调重入时不会再次转移已经移出的数量
            int remaining = item.ItemStack;
            target.ItemAdded?.Invoke(storedItem, count);
            ItemRemoved?.Invoke(item, remaining);
            return true;
        }
    }
}
