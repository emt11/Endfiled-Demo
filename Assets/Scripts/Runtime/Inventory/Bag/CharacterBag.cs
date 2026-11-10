using System;
using System.Collections.Generic;
using UnityEngine;

namespace GP
{
    public class CharacterBag : MonoBehaviour
    {
        private readonly Dictionary<ItemCategory, ItemInventory> m_ItemInventory = new Dictionary<ItemCategory, ItemInventory>();
        private readonly Dictionary<CurrencyCategory, RuntimeCurrency> m_Currencies = new Dictionary<CurrencyCategory, RuntimeCurrency>();

        public event Action<RuntimeItem, int> ItemAdded;
        public event Action<RuntimeItem, int> ItemRemoved;
        public event Action<CurrencyCategory, int> CurrencyChanged;

        private void OnDestroy()
        {
            foreach (var inventory in m_ItemInventory.Values)
            {
                inventory.Dispose();
            }

            foreach (var keyValuePair in m_Currencies)
            {
                keyValuePair.Value.Dispose();
            }

            ItemAdded = null;
            ItemRemoved = null;
            CurrencyChanged = null;

            m_ItemInventory.Clear();
            m_Currencies.Clear();
        }

        public IReadOnlyList<RuntimeItem> GetItems()
        {
            var items = new List<RuntimeItem>();
            foreach (var inventory in m_ItemInventory.Values) items.AddRange(inventory.GetActiveItems());
            return items.AsReadOnly();
        }

        public IReadOnlyList<RuntimeCurrency> GetCurrencies()
        {
            return new List<RuntimeCurrency>(m_Currencies.Values).AsReadOnly();
        }

        public bool CanAddItems(IReadOnlyList<ItemTable> items)
        {
            return TryPrepareItems(items, out _);
        }

        public bool TryAddItems(IReadOnlyList<ItemTable> items)
        {
            if (!TryPrepareItems(items, out var totals)) return false;
            var addedItems = new List<(ItemInventory inventory, RuntimeItem item, int count)>();
            foreach (var pair in totals)
                GetOrCreateInventory(pair.Key.ItemCategory).AddBatchItem(pair.Key, pair.Value, addedItems);

            // 所有分类写入完毕后再通知，事件回调只能看到完整产物。
            foreach (var added in addedItems)
                added.inventory.NotifyItemAdded(added.item, added.count);
            return true;
        }

        public int AddItem(RuntimeItem item)
        {
            if (item == null) return 0;
            return GetOrCreateInventory(item.ItemCategory).AddRuntimeItem(item);
        }

        public int AddItem(ItemAsset itemAsset, int count = 1)
        {
            if (!itemAsset || count <= 0) return 0;
            return GetOrCreateInventory(itemAsset.ItemCategory).AddRuntimeItem(itemAsset, count);
        }

        public void RemoveItem(ItemCategory itemCategory, int id, int count = 1)
        {
            if (!m_ItemInventory.TryGetValue(itemCategory, out var inventory)) return;

            inventory.RemoveRuntimeItem(id, count);
        }

        public void RemoveItem(RuntimeItem item, int count = 1)
        {
            if (item == null) return;

            RemoveItem(item.ItemCategory, item.RuntimeItemId, count);
        }

        public bool TransferItem(CharacterBag targetBag, RuntimeItem item, int itemStackCount = 1)
        {
            if (!targetBag || targetBag == this) return false;
            if (item == null) return false;
            if (!m_ItemInventory.TryGetValue(item.ItemCategory, out var inventory)) return false;

            return inventory.TransferItemTo(targetBag.GetOrCreateInventory(item.ItemCategory), item, itemStackCount);
        }

        public void AddCurrency(CurrencyAsset currencyAsset, int count = 1)
        {
            if (!currencyAsset || count <= 0 || currencyAsset.CurrencyValue <= 0
                || currencyAsset.CurrencyCategory == CurrencyCategory.None) return;
            if (m_Currencies.TryGetValue(currencyAsset.CurrencyCategory, out var currency))
            {
                currency.AddCurrency(currencyAsset, count);
                return;
            }

            var newCurrency = new RuntimeCurrency(currencyAsset);
            newCurrency.CurrencyWorthChanged += value => CurrencyChanged?.Invoke(newCurrency.Category, value);
            m_Currencies.Add(currencyAsset.CurrencyCategory, newCurrency);
            newCurrency.AddCurrency(currencyAsset, count);
        }

        public void RemoveCurrency(CurrencyAsset currencyAsset, int amount)
        {
            if (!currencyAsset) return;
            RemoveCurrency(currencyAsset.CurrencyCategory, amount);
        }

        public void RemoveCurrency(CurrencyCategory category, int amount)
        {
            if (m_Currencies.TryGetValue(category, out var currency))
            {
                currency.SubtractCurrency(amount);
            }
        }

        public RuntimeCurrency GetCurrency(CurrencyCategory category)
        {
            return m_Currencies.GetValueOrDefault(category);
        }

        public bool TrySpendCurrency(CurrencyCategory category, int amount)
        {
            if (category == CurrencyCategory.None || amount < 0) return false;
            if (amount == 0) return true;
            return m_Currencies.TryGetValue(category, out var currency) && currency.TrySpend(amount);
        }

        private bool TryPrepareItems(IReadOnlyList<ItemTable> items, out Dictionary<ItemAsset, int> totals)
        {
            totals = new Dictionary<ItemAsset, int>();
            if (items == null || items.Count == 0) return false;
            foreach (var item in items)
            {
                if (item == null || !item.ItemAsset || item.ItemCount <= 0) return false;
                long count = (long)totals.GetValueOrDefault(item.ItemAsset) + item.ItemCount;
                if (count > int.MaxValue) return false;
                totals[item.ItemAsset] = (int)count;
            }

            var slots = new Dictionary<ItemCategory, int>();
            foreach (var pair in totals)
            {
                var asset = pair.Key;
                m_ItemInventory.TryGetValue(asset.ItemCategory, out var inventory);
                int required;
                if (inventory != null)
                {
                    if (inventory.GetAddableCount(asset, pair.Value) != pair.Value) return false;
                    required = inventory.GetRequiredSlots(asset, pair.Value);
                }
                else
                {
                    if (asset.IsStackable && pair.Value > asset.MaxStackSize) return false;
                    required = asset.IsStackable ? 1 : pair.Value;
                }
                long used = (long)slots.GetValueOrDefault(asset.ItemCategory) + required;
                if (used > (inventory?.AvailableSlots ?? ItemInventory.DefaultCapacity)) return false;
                slots[asset.ItemCategory] = (int)used;
            }
            return true;
        }

        private ItemInventory GetOrCreateInventory(ItemCategory category)
        {
            if (m_ItemInventory.TryGetValue(category, out var inventory)) return inventory;
            inventory = new ItemInventory(this, category);
            inventory.ItemAdded += HandleItemAdded;
            inventory.ItemRemoved += HandleItemRemoved;
            m_ItemInventory.Add(category, inventory);
            return inventory;
        }

        private void HandleItemAdded(RuntimeItem item, int itemStackCount = 1)
        {
            ItemAdded?.Invoke(item, itemStackCount);
        }

        private void HandleItemRemoved(RuntimeItem item, int itemStackCount = 1)
        {
            ItemRemoved?.Invoke(item, itemStackCount);
        }

    }
}
