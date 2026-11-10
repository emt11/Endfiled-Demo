using System;

namespace GP
{
    /// <summary>只保留最近一次有效输入；新输入覆盖旧输入，消费成功或过期后清空</summary>
    public sealed class MeleeInputBuffer
    {
        public const float DefaultLifetime = 0.5f;
        private readonly float m_Lifetime;
        private MeleeInputCommand m_Command;
        private float m_WrittenAt;
        private bool m_HasCommand;

        public MeleeInputBuffer(float lifetime = DefaultLifetime)
        {
            if (float.IsNaN(lifetime) || float.IsInfinity(lifetime) || lifetime < 0f)
                throw new ArgumentOutOfRangeException(nameof(lifetime));
            m_Lifetime = lifetime;
        }

        public void Write(MeleeInputCommand command, float currentTime)
        {
            if (!command.IsValid || float.IsNaN(currentTime) || float.IsInfinity(currentTime)) return;
            m_Command = command;
            m_WrittenAt = currentTime;
            m_HasCommand = true;
        }

        /// <summary>取出并移除未过期输入；成功仅表示取得命令，不代表攻击已执行</summary>
        public bool TryConsume(float currentTime, out MeleeInputCommand command)
        {
            command = default;
            if (!m_HasCommand) return false;
            var age = currentTime - m_WrittenAt;
            if (float.IsNaN(age) || age < 0f || age > m_Lifetime)
            {
                Clear();
                return false;
            }

            command = m_Command;
            Clear();
            return true;
        }

        public void Clear()
        {
            m_HasCommand = false;
            m_Command = default;
        }
    }
}
