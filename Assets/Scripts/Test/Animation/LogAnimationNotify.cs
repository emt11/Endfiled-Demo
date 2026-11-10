using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [LabelText("📡日志动画通知")]
    [System.Serializable]
    public class LogAnimationNotify : AnimationNotify
    {
        [SerializeField, BoxGroup("Notify", showLabel: false)]
        private string m_Log;

        public override void TriggerNotify(GameObject user)
        {
            EMTLog.Log(m_Log);
        }
    }
}
