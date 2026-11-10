

namespace GP
{
    public static class EventCenter<TValue> where TValue : struct, IEventContext
    {
        private static readonly GameEventTokenBinder s_EventTokenBinder = new();
        private static event EventDelegate EventChannel;

        public static void Publish(TValue @event)
        {
            EventChannel?.Invoke(@event);
        }

        public static IEventToken Subscribe(EventDelegate @event)
        {
            if (@event == null) return null;

            EventChannel += @event; // 注册事件

            var newToken = new GameEventToken<TValue>(@event); // 创建事件令牌

            s_EventTokenBinder.AddEventToken(newToken); // 添加事件令牌

            return newToken; // 返回事件令牌
        }

        internal static bool UnsubscribeToken(EventDelegate @event, IEventToken eventToken)
        {
            if (@event == null) return false;

            EventChannel -= @event;

            // 同步销毁令牌，保证事件存在令牌也存在，事件销毁令牌也销毁
            // 避免出现令牌销毁了，事件还在
            return s_EventTokenBinder.RemoveEventToken(eventToken);
        }
        public delegate void EventDelegate(TValue @event);
    }

    public interface IEventContext { }

    public interface IEventToken
    {
        bool IsDisposed { get; }

        void Unsubscribe();
    }
}
