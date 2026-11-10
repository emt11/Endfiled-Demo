
using UnityEngine.InputSystem;

namespace GP
{
    public class ValueInput<TValue> : EnhancedInput where TValue : struct
    {
        public ValueInput(InputAction runtimeInputAction) : base(runtimeInputAction)
        {
            m_InputManager.AddInput(this);
        }

        public override void Enable()
        {
            if (!IsDisposed) IsInputEnabled = true;
        }

        public override void Disable()
        {
            IsInputEnabled = false;
        }

        public override void Dispose()
        {
            if (IsDisposed) return;
            Disable();
            IsDisposed = true;
            m_RuntimeInputAction = null;
            if (m_InputManager) m_InputManager.RemoveInput(this);
        }

        public TValue ReadInputValue()
        {
            return IsInputEnabled ? m_RuntimeInputAction.ReadValue<TValue>() : default(TValue);
        }
    }
}
