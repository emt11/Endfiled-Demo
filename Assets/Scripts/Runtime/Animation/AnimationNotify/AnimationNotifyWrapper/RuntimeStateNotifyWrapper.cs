using UnityEngine;

namespace GP
{
    /// <summary>管理一次状态通知的进入、更新、退出和打断</summary>
    public class RuntimeStateNotifyWrapper
    {
        private readonly GameObject m_Target;

        public AnimationStateNotify AnimationStateNotify { get; }
        public int EnterFrame { get; }
        public int ExitFrame { get; }
        public bool HasEntered { get; private set; }
        public bool HasExited { get; private set; }
        public bool IsInterrupted { get; private set; }
        public int SourceIndex { get; set; }

        public RuntimeStateNotifyWrapper(AnimationStateNotify animationStateNotify, GameObject target)
        {
            AnimationStateNotify = animationStateNotify;
            EnterFrame = animationStateNotify?.StartFrame ?? -1;
            ExitFrame = animationStateNotify?.EndFrame ?? -1;
            m_Target = target;
        }

        public void TriggerEnterNotify(GameObject target = null)
        {
            if (AnimationStateNotify == null || HasEntered || HasExited || IsInterrupted) return;
            if (target != null && target != m_Target) return;

            HasEntered = true;
            AnimationStateNotify.OnEnterNotify(m_Target);
        }

        public void TriggerTickNotify(GameObject target = null)
        {
            if (!HasEntered || HasExited || IsInterrupted) return;
            if (target != null && target != m_Target) return;

            AnimationStateNotify.OnTickNotify(m_Target);
        }

        public void TriggerExitNotify(GameObject target = null)
        {
            if (!HasEntered || HasExited) return;
            if (target != null && target != m_Target) return;

            HasExited = true;
            AnimationStateNotify.OnExitNotify(m_Target);
        }

        public void Interrupt()
        {
            if (IsInterrupted) return;

            IsInterrupted = true;
            // 尚未进入或已经退出的状态不应收到额外的 Exit
            TriggerExitNotify();
        }
    }
}
