using System.Collections.Generic;

namespace GP
{
    public class GameEventTokenBinder
    {
        private readonly HashSet<IEventToken> m_RuntimeTokens = new();

        public void AddEventToken(IEventToken eventToken)
        {
            if (eventToken == null || eventToken.IsDisposed) return;

            m_RuntimeTokens.Add(eventToken);
        }

        public bool RemoveEventToken(IEventToken eventToken)
        {
            return eventToken != null && m_RuntimeTokens.Remove(eventToken);
        }

        public bool ContainsToken(IEventToken eventToken)
        {
            return m_RuntimeTokens.Contains(eventToken);
        }

        public void DisposeAllEventTokens()
        {
            var tokens = new List<IEventToken>(m_RuntimeTokens);
            m_RuntimeTokens.Clear();

            foreach (var runtimeToken in tokens)
            {
                runtimeToken?.Unsubscribe();
            }
        }

    }
}
