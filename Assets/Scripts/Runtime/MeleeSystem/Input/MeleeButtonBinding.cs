using System;

namespace GP
{
    public sealed class MeleeButtonBinding : IDisposable
    {
        private readonly EnhancedButton m_Button;
        private EnhancedInputHold m_HoldInteraction;
        private MeleeInputSignal m_Signal;
        private bool m_IsBound;

        public event Action<MeleeInputCommand> CommandTriggered;

        public MeleeButtonBinding(EnhancedButton button)
        {
            m_Button = button ?? throw new ArgumentNullException(nameof(button));
        }

        public void Bind(WeaponInputConfig config)
        {
            Unbind();
            if (config == null || m_Button.IsDisposed) return;
            var command = new MeleeInputCommand(config.Mode, config.InputSignal);
            if (!command.IsValid) return;
            m_Signal = config.InputSignal;
            m_IsBound = true;

            if (config.Mode == MeleeInputTrigger.Charge)
            {
                m_HoldInteraction = new EnhancedInputHold(config.HoldDuration, config.AutoTrigger);
                m_HoldInteraction.InteractionTriggered += HandleCharge;
                m_Button.SetInteraction(m_HoldInteraction);
                m_Button.ButtonReleased += HandleTap;
            }
            else
            {
                m_Button.ButtonPressed += HandleTap;
            }
        }

        public void Unbind()
        {
            m_IsBound = false;
            m_Button.ButtonPressed -= HandleTap;
            m_Button.ButtonReleased -= HandleTap;
            if (m_HoldInteraction != null)
            {
                m_HoldInteraction.InteractionTriggered -= HandleCharge;
                m_Button.RemoveInteraction(m_HoldInteraction);
                m_HoldInteraction = null;
            }
            m_Signal = MeleeInputSignal.None;
        }

        public void Dispose()
        {
            Unbind();
            CommandTriggered = null;
        }

        private void HandleTap()
        {
            if (m_IsBound) CommandTriggered?.Invoke(new MeleeInputCommand(MeleeInputTrigger.Tap, m_Signal));
        }

        private void HandleCharge()
        {
            if (m_IsBound) CommandTriggered?.Invoke(new MeleeInputCommand(MeleeInputTrigger.Charge, m_Signal));
        }
    }
}
