using System.Collections.Generic;
using UnityEngine;

namespace GP
{
    public class GameInputManager : SingletonMono<GameInputManager>
    {

        private GP_InputAction m_InputActions;

        private readonly HashSet<EnhancedInput> m_Inputs = new HashSet<EnhancedInput>();
        // 当前输入系统中被施加交互组件的活跃按键
        private readonly HashSet<IEnhancedInputInteractionTick> m_InteractionTicks = new HashSet<IEnhancedInputInteractionTick>();
        // 等待被移除的交互组件
        private readonly HashSet<IEnhancedInputInteractionTick> m_PendingInteractionRemovals = new();
        private readonly List<IEnhancedInputInteractionTick> m_InteractionTickSnapshot = new();
        protected override bool IsPersistent => true;

        public ValueInput<Vector2> MovementInput { get; private set; }
        public ValueInput<Vector2> LookInput { get; private set; }

        public EnhancedButton QKey { get; private set; }
        public EnhancedButton EKey { get; private set; }
        public EnhancedButton FKey { get; private set; }
        public EnhancedButton LMB { get; private set; }
        public EnhancedButton RMB { get; private set; }

        private void OnEnable()
        {
            if (m_InputActions == null) return;

            foreach (var input in m_Inputs)
            {
                input.Enable();
            }
            m_InputActions.Enable();
        }

        private void Update()
        {
            TickInteractions();
        }

        private void OnDisable()
        {
            foreach (var input in m_Inputs)
            {
                input.Disable();
            }
            m_InputActions?.Disable();
        }

        protected override void OnDestroy()
        {
            var inputs = new List<EnhancedInput>(m_Inputs);
            m_Inputs.Clear();
            foreach (var input in inputs)
            {
                input.Dispose();
            }
            m_InteractionTicks.Clear();
            m_PendingInteractionRemovals.Clear();
            m_InteractionTickSnapshot.Clear();
            m_InputActions?.Dispose();
            m_InputActions = null;
            base.OnDestroy();
        }

        public void InitializeInputs()
        {
            if (m_InputActions != null) return;
            m_InputActions = new GP_InputAction();

            MovementInput = new ValueInput<Vector2>(m_InputActions.GamePlay.MovementKey);
            LookInput = new ValueInput<Vector2>(m_InputActions.GamePlay.MouseLook);

            QKey = new EnhancedButton(m_InputActions.GamePlay.Q);
            EKey = new EnhancedButton(m_InputActions.GamePlay.E);
            FKey = new EnhancedButton(m_InputActions.GamePlay.F);
            LMB = new EnhancedButton(m_InputActions.GamePlay.LMB);
            RMB = new EnhancedButton(m_InputActions.GamePlay.RMB);
        }

        public void AddInput(EnhancedInput input)
        {
            if (input == null || input.IsDisposed || !m_Inputs.Add(input)) return;
            if (isActiveAndEnabled) input.Enable();
        }

        public void RemoveInput(EnhancedInput input)
        {
            if (input == null) return;
            m_Inputs.Remove(input);
        }

        public void AddInteractionTick(IEnhancedInputInteractionTick tick)
        {
            if (tick == null) return;

            tick.IsPendingRemoval = false;
            m_PendingInteractionRemovals.Remove(tick);
            m_InteractionTicks.Add(tick);
        }

        public void RemoveInteractionTick(IEnhancedInputInteractionTick tick)
        {
            if (tick == null) return;

            tick.IsPendingRemoval = true;
            m_PendingInteractionRemovals.Add(tick);
        }

        protected override void AwakeSingleton()
        {
            InitializeInputs();
        }

        private void TickInteractions()
        {
            m_InteractionTickSnapshot.Clear();
            m_InteractionTickSnapshot.AddRange(m_InteractionTicks);
            for (int i = 0; i < m_InteractionTickSnapshot.Count; i++)
            {
                var interactionTick = m_InteractionTickSnapshot[i];
                if (interactionTick.IsPendingRemoval) continue;

                interactionTick.TickInteraction();
            }

            if (m_PendingInteractionRemovals.Count > 0)
            {
                foreach (var removedInteractionTick in m_PendingInteractionRemovals)
                {
                    m_InteractionTicks.Remove(removedInteractionTick);
                }

                m_PendingInteractionRemovals.Clear();
            }
        }
    }
}
