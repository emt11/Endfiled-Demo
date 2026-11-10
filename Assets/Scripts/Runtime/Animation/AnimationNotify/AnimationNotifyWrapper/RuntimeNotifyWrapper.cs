using UnityEngine;

namespace GP
{
    /// <summary>保存一次播放中的通知状态，不修改共享的 AnimationAsset</summary>
    public class RuntimeNotifyWrapper
    {
        private readonly GameObject m_Target;

        public AnimationNotify AnimationNotify { get; private set; }
        public int TriggerFrame { get; private set; }
        public bool HasTriggered { get; private set; }
        public int SourceIndex { get; set; }

        public RuntimeNotifyWrapper(AnimationNotify animationNotify, GameObject target)
        {
            m_Target = target;
            Reset(animationNotify);
        }

        public void TriggerNotify(GameObject target = null)
        {
            if (HasTriggered || AnimationNotify == null) return;
            if (target != null && target != m_Target) return;

            // 先更新状态，避免通知回调再次触发自身
            HasTriggered = true;
            AnimationNotify.TriggerNotify(m_Target);
        }

        public void Reset(AnimationNotify animationNotify)
        {
            AnimationNotify = animationNotify;
            TriggerFrame = animationNotify?.TriggerFrame ?? -1;
            HasTriggered = false;
        }
    }
}
