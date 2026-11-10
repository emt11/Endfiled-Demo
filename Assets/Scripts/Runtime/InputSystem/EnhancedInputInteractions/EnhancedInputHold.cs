using System;
using UnityEngine;

namespace GP
{
    public class EnhancedInputHold : IEnhancedInputInteractionTick
    {
        private readonly float m_HoldTime; // 长按时长
        private readonly bool m_AutoTrigger; // 是否自动触发

        private float m_EndTime; // 达到长按时长的截止时间
        private bool m_IsInteracting; // 是否按下/正在交互
        private bool m_HasTriggered; // 本轮交互是否已经触发

        public event Action InteractionStarted;
        public event Action InteractionTriggered;

        public bool IsPendingRemoval { get; set; }

        public EnhancedInputHold(float holdTime, bool autoTrigger)
        {
            if (holdTime < 0f || float.IsNaN(holdTime) || float.IsInfinity(holdTime))
                throw new ArgumentOutOfRangeException(nameof(holdTime));
            m_HoldTime = holdTime;
            m_AutoTrigger = autoTrigger;
        }

        public bool BeginInteraction()
        {
            m_EndTime = Time.time + m_HoldTime;
            m_IsInteracting = true;
            m_HasTriggered = false;
            InteractionStarted?.Invoke();
            return m_IsInteracting;
        }

        public void ResetInteraction()
        {
            m_IsInteracting = false;
            m_HasTriggered = false;
            m_EndTime = 0f;
        }

        public bool EndInteraction()
        {
            // 本轮已经触发，松开时返回“已处理”
            if (m_HasTriggered) return true;

            if (!m_IsInteracting) return false;

            // 松开就结束计时，后续轮询不会再触发
            m_IsInteracting = false;

            // 时长不足，按短按处理
            if (Time.time < m_EndTime) return false;

            // 时长已满足，触发一次
            return Trigger();
        }

        // 轮询
        public void TickInteraction()
        {
            if (IsPendingRemoval || !m_IsInteracting || !m_AutoTrigger) return;

            if (Time.time >= m_EndTime)
            {
                Trigger();
            }
        }

        private bool Trigger()
        {
            m_IsInteracting = false;
            m_HasTriggered = true;
            InteractionTriggered?.Invoke();

            return true;
        }
    }
}
