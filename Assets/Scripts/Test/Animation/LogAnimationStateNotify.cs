using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [LabelText("📡日志状态通知")]
    [System.Serializable]
    public class LogAnimationStateNotify : AnimationStateNotify
    {
        [Space, SerializeField, BoxGroup("Log", showLabel: false)]
        private string m_EnterLog;
        [Space, SerializeField, BoxGroup("Log", showLabel: false)]
        private string m_TickLog;
        [Space, SerializeField, BoxGroup("Log", showLabel: false)]
        private string m_ExitLog;

        public override void OnEnterNotify(GameObject user)
        {
            EMTLog.Log(m_EnterLog);
        }

        public override void OnTickNotify(GameObject user)
        {
            EMTLog.Log(m_TickLog);
        }

        public override void OnExitNotify(GameObject user)
        {
            EMTLog.Log(m_ExitLog);
        }
    }
}
