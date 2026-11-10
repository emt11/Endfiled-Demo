using System;


namespace GP
{
    public class RuntimeCurrency : IDisposable
    {
        private const int MaxCurrencyWorth = 99999;
        private readonly CurrencyAsset m_CurrencyAsset;

        private int m_CurrencyWorth; // 也就是该财富类型的总金额

        public CurrencyCategory Category => m_CurrencyAsset.CurrencyCategory;
        public int CurrencyWorth => m_CurrencyWorth;
        public event Action<int> CurrencyWorthChanged;

        public RuntimeCurrency(CurrencyAsset currencyAsset)
        {
            if (!currencyAsset) throw new ArgumentNullException(nameof(currencyAsset));
            if (currencyAsset.CurrencyCategory == CurrencyCategory.None)
                throw new ArgumentException("货币资产必须配置有效币种。", nameof(currencyAsset));
            m_CurrencyAsset = currencyAsset;
        }

        public void AddCurrency(int amount)
        {
            if (amount <= 0) return;
            SetWorth((long)m_CurrencyWorth + amount);
        }

        public void AddCurrency(CurrencyAsset currencyAsset, int count = 1)
        {
            if (!currencyAsset || count <= 0 || currencyAsset.CurrencyValue <= 0
                || currencyAsset.CurrencyCategory != Category) return;
            SetWorth(m_CurrencyWorth + (long)currencyAsset.CurrencyValue * count);
        }

        public void SubtractCurrency(int amount)
        {
            if (amount <= 0) return;
            SetWorth((long)m_CurrencyWorth - amount);
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0 || amount > m_CurrencyWorth) return false;
            if (amount > 0) SetWorth(m_CurrencyWorth - amount);
            return true;
        }

        public void Dispose()
        {
            CurrencyWorthChanged = null;
        }

        private void SetWorth(long amount)
        {
            int value = (int)Math.Max(0L, Math.Min(amount, MaxCurrencyWorth));
            if (value == m_CurrencyWorth) return;
            m_CurrencyWorth = value;
            CurrencyWorthChanged?.Invoke(value);
        }
    }
}
