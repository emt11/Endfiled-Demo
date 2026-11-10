using UnityEngine;

namespace GP
{
    public enum ItemCategory
    {
        None,
        [InspectorName("🗡️武器")] Weapon,
        [InspectorName("🛡️护甲")] Armor,
        [InspectorName("⚗️消耗品")] Consumable,
        [InspectorName("🍗食物")] Food,
        [InspectorName("⛏️材料")] Material,
    }
}
