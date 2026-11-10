
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [DisallowMultipleComponent, RequireComponent(typeof(MeleeCombatController))]
    public class MeleeInputHandler : MonoBehaviour
    {
        [SerializeField, Min(0.1f), LabelText("锁定搜索半径")]
        private float m_TargetRadius = 10f;
        [SerializeField, LabelText("可锁定目标层")]
        private LayerMask m_TargetLayers = ~0;
        [SerializeField, LabelText("锁定相机（留空使用主相机）")]
        private ThirdPersonCamera m_Camera;

        private MeleeCombatController m_CombatController;
        private DamageReceiver m_DamageReceiver;
        private GameInputManager m_InputManager;
        private MeleeTargeting m_Targeting;
        private IEventToken m_TargetSubscription;
        private readonly List<MeleeButtonBinding> m_Bindings = new List<MeleeButtonBinding>();
        private bool m_HasStarted;

        private void Awake()
        {
            m_CombatController = GetComponent<MeleeCombatController>();
            m_DamageReceiver = GetComponent<DamageReceiver>();
            m_Targeting = new MeleeTargeting(transform, m_TargetRadius, m_TargetLayers);
        }

        private void OnEnable()
        {
            m_CombatController.WeaponChanged += HandleWeaponChanged;
            m_Targeting.FocusTargetChanged += HandleFocusTargetChanged;
            m_TargetSubscription = EventCenter<TargetUnavailableEvent>.Subscribe(HandleTargetUnavailable);
            if (m_HasStarted)
            {
                BindCombatInputs();
                BindCurrentWeapon();
            }
        }

        private void Start()
        {
            // 所有 Awake 已完成；先创建绑定再读取当前武器，不依赖其他组件的 Start 顺序
            m_InputManager = GameInputManager.Instance;
            if (!m_InputManager) return;
            if (!m_Camera && Camera.main) m_Camera = Camera.main.GetComponentInParent<ThirdPersonCamera>();
            m_Bindings.Add(new MeleeButtonBinding(m_InputManager.LMB));
            m_Bindings.Add(new MeleeButtonBinding(m_InputManager.RMB));
            m_Bindings.Add(new MeleeButtonBinding(m_InputManager.FKey));
            foreach (var binding in m_Bindings) binding.CommandTriggered += HandleCommand;
            m_HasStarted = true;
            BindCombatInputs();
            BindCurrentWeapon();
        }

        private void Update()
        {
            if (m_DamageReceiver && m_DamageReceiver.IsDead)
            {
                m_Targeting.ClearTarget();
                return;
            }
            m_Targeting.RefreshFocusTarget();
            if (!m_InputManager || m_Targeting.FocusTarget == null) return;
            var scroll = m_InputManager.ScrollInput.ReadInputValue().y;
            if (scroll < 0f) m_Targeting.NextTarget();
            else if (scroll > 0f) m_Targeting.PreviousTarget();
        }

        private void OnDisable()
        {
            if (m_CombatController) m_CombatController.WeaponChanged -= HandleWeaponChanged;
            foreach (var binding in m_Bindings) binding.Unbind();
            if (m_HasStarted)
            {
                m_InputManager.SpaceKey.ButtonPressed -= HandleDefensePressed;
                m_InputManager.SpaceKey.ButtonReleased -= HandleDefenseReleased;
                m_InputManager.MMB.ButtonPressed -= ToggleTargetLock;
            }
            if (m_DamageReceiver) m_DamageReceiver.EndDefense();
            m_TargetSubscription?.Unsubscribe();
            m_TargetSubscription = null;
            m_Targeting.ClearTarget();
            m_Targeting.FocusTargetChanged -= HandleFocusTargetChanged;
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

        private void BindCombatInputs()
        {
            m_InputManager.SpaceKey.ButtonPressed += HandleDefensePressed;
            m_InputManager.SpaceKey.ButtonReleased += HandleDefenseReleased;
            m_InputManager.MMB.ButtonPressed += ToggleTargetLock;
        }

        private void HandleDefensePressed()
        {
            if (m_DamageReceiver) m_DamageReceiver.TryStartDefense();
        }

        private void HandleDefenseReleased()
        {
            if (m_DamageReceiver) m_DamageReceiver.EndDefense();
        }

        private void ToggleTargetLock()
        {
            if (m_DamageReceiver && m_DamageReceiver.IsDead) return;
            if (m_Targeting.FocusTarget == null) m_Targeting.FindTargets();
            else m_Targeting.ClearTarget();
        }

        private void HandleFocusTargetChanged(IDamageable target)
        {
            m_CombatController.SetFocusTarget(target);
            if (m_Camera) m_Camera.SetFocusTarget(m_CombatController.FocusTarget);
        }

        private void HandleTargetUnavailable(TargetUnavailableEvent context)
        {
            // 如果失效的 Target 就是目前正在锁定的 Target ，那么重新寻找
            if (ReferenceEquals(m_Targeting.FocusTarget, context.Target)) m_Targeting.FindTargets();
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

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, m_TargetRadius);
        }
    }
}
