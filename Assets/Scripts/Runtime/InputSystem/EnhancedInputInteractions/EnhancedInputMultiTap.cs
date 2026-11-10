using System;
using UnityEngine;

namespace GP
{
    public class EnhancedInputMultiTap : IEnhancedInputInteraction
    {
        private readonly int m_TapCount; // 连击成功次数
        private readonly float m_MaxTapInterval; // 两次点击的有效间隔时间

        private int m_CurrentTapCount;
        private float m_LastTapTime;

        public event Action InteractionStarted;
        public event Action InteractionTriggered;

        public EnhancedInputMultiTap(int tapCount, float maxTapInterval)
        {
            if (tapCount < 1)
                throw new ArgumentOutOfRangeException(nameof(tapCount));

            if (maxTapInterval < 0f || float.IsNaN(maxTapInterval) || float.IsInfinity(maxTapInterval))
                throw new ArgumentOutOfRangeException(nameof(maxTapInterval));

            m_TapCount = tapCount;
            m_MaxTapInterval = maxTapInterval;
        }

        public bool BeginInteraction()
        {
            float currentTime = Time.time;

            bool isValidTap = m_CurrentTapCount == 0 || currentTime - m_LastTapTime < m_MaxTapInterval;
            m_CurrentTapCount = isValidTap ? m_CurrentTapCount + 1 : 1;
            m_LastTapTime = currentTime;

            bool hasTriggered = m_CurrentTapCount == m_TapCount;
            if (hasTriggered) m_CurrentTapCount = 0;

            InteractionStarted?.Invoke();
            if (hasTriggered)
            {
                InteractionTriggered?.Invoke();
            }

            return isValidTap;
        }

        public bool EndInteraction()
        {
            return true;
        }

        public void ResetInteraction()
        {
            m_CurrentTapCount = 0;
            m_LastTapTime = 0f;
        }
    }
}
