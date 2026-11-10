using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace GP
{
    [CreateAssetMenu(fileName = "New ValueAttributeAsset", menuName = "GP/ValueAttribute")]
    public class ValueAttributeAsset : ScriptableObject
    {
        [SerializeField, BoxGroup("Settings", showLabel: false)]
        private ValueAttributeName m_AttributeName;

        [FormerlySerializedAs("m_Value")]
        [SerializeField, Min(0), BoxGroup("Settings", showLabel: false)]
        private int m_BaseValue;

        public ValueAttributeName AttributeName => m_AttributeName;
        public int BaseValue => m_BaseValue;
    }

    public enum ValueAttributeName
    {
        Health,
        Mana,
        Defense,
        Damage,
        Stamina
    }
}
