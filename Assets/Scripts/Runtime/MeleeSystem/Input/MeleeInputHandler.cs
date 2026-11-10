
using System.Collections.Generic;
using UnityEngine;

namespace GP
{
    [DisallowMultipleComponent, RequireComponent(typeof(MeleeCombatController))]
    public class MeleeInputHandler : MonoBehaviour
    {
        private MeleeCombatController m_CombatController;
        private readonly List<MeleeButtonBinding> m_Bindings = new List<MeleeButtonBinding>();
        private bool m_HasStarted;

        private void Awake()
        {
            m_CombatController = GetComponent<MeleeCombatController>();
        }

        private void OnEnable()
        {
            m_CombatController.WeaponChanged += HandleWeaponChanged;
            if (m_HasStarted) BindCurrentWeapon();
        }

        private void Start()
        {
            // 所有 Awake 已完成；先创建绑定再读取当前武器，不依赖其他组件的 Start 顺序
            var inputManager = GameInputManager.Instance;
            if (!inputManager) return;
            m_Bindings.Add(new MeleeButtonBinding(inputManager.LMB));
            m_Bindings.Add(new MeleeButtonBinding(inputManager.RMB));
            m_Bindings.Add(new MeleeButtonBinding(inputManager.FKey));
            foreach (var binding in m_Bindings) binding.CommandTriggered += HandleCommand;
            m_HasStarted = true;
            BindCurrentWeapon();
        }

        private void OnDisable()
        {
            if (m_CombatController) m_CombatController.WeaponChanged -= HandleWeaponChanged;
            foreach (var binding in m_Bindings) binding.Unbind();
        }

        private void OnDestroy()
        {
            foreach (var binding in m_Bindings) binding.Dispose();
            m_Bindings.Clear();
        }

        private void HandleCommand(MeleeInputCommand command)
        {
            if (isActiveAndEnabled && m_CombatController && m_CombatController.isActiveAndEnabled)
                m_CombatController.BufferInput(command);
        }

        private void HandleWeaponChanged(RuntimeWeapon weapon)
        {
            if (m_HasStarted) BindCurrentWeapon();
        }

        private void BindCurrentWeapon()
        {
            var weapon = m_CombatController.CurrentWeapon;
            if (weapon == null || !weapon.IsEquipped || m_Bindings.Count != 3)
            {
                foreach (var binding in m_Bindings) binding.Unbind();
                return;
            }
            var asset = weapon.Asset;
            m_Bindings[0].Bind(asset.PrimaryInput);
            m_Bindings[1].Bind(asset.SecondaryInput);
            m_Bindings[2].Bind(asset.UtilityInput);
        }
    }
}
