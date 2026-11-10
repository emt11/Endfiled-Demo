using System.Collections.Generic;
using UnityEngine;

namespace GP
{
    /// <summary>按帧推进通知游标，跳帧时补发经过的通知</summary>
    public class RuntimeWrapperManager
    {
        private readonly List<RuntimeNotifyWrapper> m_NotifyWrappers = new();
        private readonly List<RuntimeStateNotifyWrapper> m_StateWrappersByEnterFrame = new();
        private readonly List<RuntimeStateNotifyWrapper> m_StateWrappersByExitFrame = new();
        private int m_NotifyCursor;
        private int m_StateEnterCursor;
        private int m_StateExitCursor;
        private int m_LastEvaluatedFrame = -1;
        private bool m_IsInterrupted;

        public RuntimeWrapperManager(AnimationNotifyList notifyList, AnimationStateNotifyList stateNotifyList, GameObject target = null)
        {
            var notifies = notifyList?.Notifies;
            if (notifies != null)
            {
                for (var i = 0; i < notifies.Count; i++)
                {
                    var notify = notifies[i];
                    if (notify == null || notify.TriggerFrame < 0) continue;

                    m_NotifyWrappers.Add(new RuntimeNotifyWrapper(notify, target) { SourceIndex = i });
                }
            }

            var stateNotifies = stateNotifyList?.StateNotifies;
            if (stateNotifies != null)
            {
                for (var i = 0; i < stateNotifies.Count; i++)
                {
                    var notify = stateNotifies[i];
                    if (notify == null || notify.StartFrame < 0 || notify.EndFrame < notify.StartFrame) continue;

                    var wrapper = new RuntimeStateNotifyWrapper(notify, target) { SourceIndex = i };
                    m_StateWrappersByEnterFrame.Add(wrapper);
                    m_StateWrappersByExitFrame.Add(wrapper);
                }
            }

            // 在运行时副本中排序；同帧通知保持 Inspector 中的先后顺序
            m_NotifyWrappers.Sort((x, y) =>
            {
                var order = x.TriggerFrame.CompareTo(y.TriggerFrame);
                return order != 0 ? order : x.SourceIndex.CompareTo(y.SourceIndex);
            });
            m_StateWrappersByEnterFrame.Sort((x, y) =>
            {
                var order = x.EnterFrame.CompareTo(y.EnterFrame);
                return order != 0 ? order : x.SourceIndex.CompareTo(y.SourceIndex);
            });
            m_StateWrappersByExitFrame.Sort((x, y) =>
            {
                var order = x.ExitFrame.CompareTo(y.ExitFrame);
                return order != 0 ? order : x.SourceIndex.CompareTo(y.SourceIndex);
            });
        }

        public void EvaluateFrame(int currentFrame)
        {
            if (m_IsInterrupted || currentFrame <= m_LastEvaluatedFrame) return;

            // 按顺序补齐经过的动画帧，避免跳帧时丢失范围内的 Tick
            while (!m_IsInterrupted && m_LastEvaluatedFrame < currentFrame)
            {
                m_LastEvaluatedFrame++;
                EvaluateSingleFrame(m_LastEvaluatedFrame);
            }
        }

        public void InterruptNotifies()
        {
            if (m_IsInterrupted) return;

            m_IsInterrupted = true;
            foreach (var wrapper in m_StateWrappersByExitFrame)
                wrapper.Interrupt();
        }

        private void EvaluateSingleFrame(int currentFrame)
        {
            while (!m_IsInterrupted && m_NotifyCursor < m_NotifyWrappers.Count
                && m_NotifyWrappers[m_NotifyCursor].TriggerFrame <= currentFrame)
            {
                m_NotifyWrappers[m_NotifyCursor++].TriggerNotify();
            }

            while (!m_IsInterrupted && m_StateEnterCursor < m_StateWrappersByEnterFrame.Count
                && m_StateWrappersByEnterFrame[m_StateEnterCursor].EnterFrame <= currentFrame)
            {
                m_StateWrappersByEnterFrame[m_StateEnterCursor++].TriggerEnterNotify();
            }

            // 进入帧只 Enter，退出帧只 Exit；仅在两者之间执行 Tick
            for (var i = 0; !m_IsInterrupted && i < m_StateEnterCursor; i++)
            {
                var wrapper = m_StateWrappersByEnterFrame[i];
                if (currentFrame > wrapper.EnterFrame && currentFrame < wrapper.ExitFrame)
                    wrapper.TriggerTickNotify();
            }

            while (!m_IsInterrupted && m_StateExitCursor < m_StateWrappersByExitFrame.Count
                && m_StateWrappersByExitFrame[m_StateExitCursor].ExitFrame <= currentFrame)
            {
                m_StateWrappersByExitFrame[m_StateExitCursor++].TriggerExitNotify();
            }
        }
    }
}
