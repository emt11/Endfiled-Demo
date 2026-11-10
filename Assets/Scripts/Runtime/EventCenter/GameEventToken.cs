namespace GP
{
    public class GameEventToken<TValue> : IEventToken where TValue : struct, IEventContext
    {
        private EventCenter<TValue>.EventDelegate m_EventDelegate;
        private bool m_IsDisposed; // 可以理解为该 Token 是否还有效

        public bool IsDisposed => m_IsDisposed;

        internal GameEventToken(EventCenter<TValue>.EventDelegate runtimeEvent)
        {
            m_EventDelegate = runtimeEvent;
            m_IsDisposed = false;
        }

        public void Unsubscribe()
        {
            if (m_IsDisposed || m_EventDelegate == null) return;

            // 先更新状态，允许绑定器清空集合后再批量注销，也避免重入重复注销。
            m_IsDisposed = true;
            var eventDelegate = m_EventDelegate;
            m_EventDelegate = null;
            EventCenter<TValue>.UnsubscribeToken(eventDelegate, this);
        }

    }
}
