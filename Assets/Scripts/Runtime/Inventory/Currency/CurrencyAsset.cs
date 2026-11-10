using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [CreateAssetMenu(fileName = "New Currency Asset", menuName = "GP/Inventory/Currency Asset/Currency")]
    public class CurrencyAsset : ScriptableObject
    {
        [SerializeField, BoxGroup("Currency Info", showLabel: false), HorizontalGroup("Currency Info/Currency"), LabelText("货币名称:")]
        private string m_CurrencyName = "财富";

        [SerializeField, BoxGroup("Currency Info", showLabel: false), HorizontalGroup("Currency Info/Currency"), LabelText("货币类型:")]
        private CurrencyCategory m_Category = CurrencyCategory.None;

        [SerializeField, BoxGroup("Currency Info", showLabel: false), LabelText("货币值:")]
        private int m_CurrencyValue = 100;

        [Space, SerializeField, BoxGroup("Currency Info", showLabel: false), TextArea]
        private string m_CurrencyDescription = "玩家的财富值";

        public string CurrencyName => m_CurrencyName;
        public CurrencyCategory CurrencyCategory => m_Category;
        public int CurrencyValue => m_CurrencyValue;
        public string CurrencyDescription => m_CurrencyDescription;
    }

    public enum CurrencyCategory
    {
        None,
        [InspectorName("金子")] Gold,
        [InspectorName("银锭")] Silver,
        [InspectorName("硬币")] Coin,
        [InspectorName("凭证")] Token
    }
}
