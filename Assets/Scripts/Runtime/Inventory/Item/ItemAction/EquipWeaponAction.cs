// using System;
// using Sirenix.OdinInspector;
// using UnityEngine;

// namespace GP
// {
// [Serializable]
// public class EquipWeaponAction : IItemAction
// {
// [SerializeField,BoxGroup("WeaponAsset",showLabel:false),LabelText("武器资产:")]
// private WeaponAsset m_WeaponAsset;

// [NonSerialized] private bool m_IsEquip;

// public ItemActionCategory ActionCategory => ItemActionCategory.Equip;

// public bool CanExecute(RuntimeItem runtimeItem)
// {
// return m_WeaponAsset;
// }

// public void Execute(RuntimeItem runtimeItem)
// {
// if(runtimeItem == null) return;
// if(!m_WeaponAsset) return;

// if(!runtimeItem.ItemInventory.CharacterBag.TryGetComponent(out MeleeCombatController combatCore)) return;

// if (!m_IsEquip)
// {
// combatCore.EquipWeapon(m_WeaponAsset);
// m_IsEquip = true;
// }
// else
// {
// combatCore.UnequipWeapon();
// m_IsEquip = false;
// }
// }
// }
// }
