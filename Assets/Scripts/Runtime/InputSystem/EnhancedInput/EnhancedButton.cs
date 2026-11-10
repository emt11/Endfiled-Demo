using System;

using UnityEngine.InputSystem;

namespace GP
{
    public class EnhancedButton : EnhancedInput
    {
        private IEnhancedInputInteraction m_ButtonInteraction; // 交互：长按、多点等

        public event Action ButtonPressed;
        public event Action ButtonReleased;

        public EnhancedButton(InputAction runtimeInputAction) : base(runtimeInputAction)
        {
            m_InputManager.AddInput(this);
        }

        public override void Enable()
        {
            if (IsDisposed || IsInputEnabled) return;
            IsInputEnabled = true;
            m_RuntimeInputAction.started += HandleInputAction;
            m_RuntimeInputAction.canceled += HandleInputAction;
            if (m_ButtonInteraction is IEnhancedInputInteractionTick tick)
                m_InputManager.AddInteractionTick(tick);
        }

        public override void Disable()
        {
            if (!IsInputEnabled) return;
            IsInputEnabled = false;
            m_RuntimeInputAction.started -= HandleInputAction;
            m_RuntimeInputAction.canceled -= HandleInputAction;
            m_ButtonInteraction?.ResetInteraction();
            if (m_ButtonInteraction is IEnhancedInputInteractionTick tick)
                m_InputManager.RemoveInteractionTick(tick);
        }

        public override void Dispose()
        {
            if (IsDisposed) return;
            Disable();
            RemoveInteraction(m_ButtonInteraction);
            IsDisposed = true;
            m_RuntimeInputAction = null;
            ButtonPressed = null;
            ButtonReleased = null;
            if (m_InputManager) m_InputManager.RemoveInput(this);
        }

        public void SetInteraction(IEnhancedInputInteraction buttonInteraction)
        {
            if (IsDisposed || ReferenceEquals(m_ButtonInteraction, buttonInteraction)) return;
            RemoveInteraction(m_ButtonInteraction);

            m_ButtonInteraction = buttonInteraction;

            if (IsInputEnabled && buttonInteraction is IEnhancedInputInteractionTick tick)
            {
                m_InputManager.AddInteractionTick(tick);
            }
            EMTLog.Log("添加交互事件");
        }

        public void RemoveInteraction(IEnhancedInputInteraction buttonInteraction)
        {
            if (buttonInteraction == null) return;
            if (m_ButtonInteraction != buttonInteraction) return;

            if (buttonInteraction is IEnhancedInputInteractionTick tick)
            {
                if (m_InputManager) m_InputManager.RemoveInteractionTick(tick);
            }

            m_ButtonInteraction = null;
            buttonInteraction.ResetInteraction();
            EMTLog.Log("移除交互事件");
        }

        private void HandleInputAction(InputAction.CallbackContext ctx)
        {
            switch (ctx.phase)
            {
                case InputActionPhase.Started:
                    InvokeButtonPressedEvent();
                    m_ButtonInteraction?.BeginInteraction();
                    break;
                case InputActionPhase.Canceled:
                    var result = m_ButtonInteraction?.EndInteraction() ?? false;
                    if (!result)
                        InvokeButtonReleasedEvent();
                    break;
            }
        }

        private void InvokeButtonPressedEvent()
        {
            ButtonPressed?.Invoke();
        }

        private void InvokeButtonReleasedEvent()
        {
            ButtonReleased?.Invoke();
        }
    }
}
