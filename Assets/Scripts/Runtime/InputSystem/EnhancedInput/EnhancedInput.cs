using System;
using UnityEngine.InputSystem;

namespace GP
{
    public abstract class EnhancedInput : IDisposable
    {
        protected InputAction m_RuntimeInputAction;
        protected readonly GameInputManager m_InputManager;

        public bool IsInputEnabled { get; protected set; }
        public bool IsDisposed { get; protected set; }

        protected EnhancedInput(InputAction runtimeInputAction)
        {
            m_RuntimeInputAction = runtimeInputAction ?? throw new ArgumentNullException(nameof(runtimeInputAction));
            m_InputManager = GameInputManager.Instance;
            if (!m_InputManager)
                throw new InvalidOperationException("请在游戏运行时创建增强输入。");
        }

        public abstract void Enable();
        public abstract void Disable();
        public abstract void Dispose();
    }
}
