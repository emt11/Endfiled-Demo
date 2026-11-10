using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace GP
{
    public class OneShotAnimationState : PlayableBehaviour, IAnimationState
    {
        private float m_PlaybackTime;
        private float m_Duration;
        private float m_NormalizedTime;
        private float m_Weight;
        private AnimationRunningStatus m_RunningStatus;
        private bool m_IsInTransition;
        private bool m_IsDone;
        private int m_Frame;

        private PlayableGraph m_Graph;
        private Playable m_Self;
        private AnimationLayerMixerPlayable m_LayerMixer;
        private AnimationClipPlayable m_AnimationClipPlayable;
        private OneShotAnimationParent m_OneShotAnimationParent;

        private AnimationAsset m_AnimationAsset;

        private bool m_IsInterrupting;
        private bool m_HasInterruptRequest;
        private bool m_IsDestroyed;

        private double m_StartTime; // 该动画开始播放的时间点
        private double m_InterruptTime; // 该动画被打断的时间点
        private float m_PlaySpeed;
        private double m_EaseInEndTime;
        private double m_EaseOutStartTime;
        private double m_InterruptFadeOutDuration; // 这个时间是在动画被打断的时候调用的
        private int m_LastFrame;

        public float PlaybackTime => m_PlaybackTime;
        public float Duration => m_Duration;
        public float NormalizedTime => m_NormalizedTime;
        public float Weight => m_Weight;
        public AnimationRunningStatus RunningStatus => m_RunningStatus;
        public string AnimationName => m_AnimationAsset.AnimationName;
        public string AnimationTag => m_AnimationAsset.AnimationTag;
        public bool IsInTransition => m_IsInTransition;
        public bool IsDone => m_IsDone;
        public bool UseRootMotion => m_AnimationAsset.UseRootMotion;
        public int Frame => m_Frame;

        public void OnDestroy()
        {
            if (!m_Self.IsValid()) return;
            if (m_IsDestroyed) return;

            if (!m_IsDone)
                m_OneShotAnimationParent.InterruptAnimationNotify(this);

            if (m_LayerMixer.IsValid()) m_LayerMixer.Destroy();
            if (m_AnimationClipPlayable.IsValid()) m_AnimationClipPlayable.Destroy();
            m_Self.Destroy();

            m_OneShotAnimationParent.RemoveAnimationState(this);
            m_IsDestroyed = true;
        }

        public static ScriptPlayable<OneShotAnimationState> Create(PlayableGraph graph, AnimationAsset animationAsset)
        {
            var handle = ScriptPlayable<OneShotAnimationState>.Create(graph, 1);
            handle.GetBehaviour().Init(graph, animationAsset);
            return handle;
        }

        public override void OnPlayableCreate(Playable playable)
        {
            m_Self = playable;
        }

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            m_PlaybackTime = (float)playable.GetTime(); // 读取当前 Playable 节点的本地播放时间
            m_NormalizedTime = m_Duration > 0f ? Mathf.Clamp01(m_PlaybackTime / m_AnimationAsset.AnimationClip.length) : 0f;
            // 计算动画的总帧数
            var totalFrames = Mathf.Max(1, AnimationTimeUtility.ToFrames(m_AnimationAsset.AnimationClip.length, m_AnimationAsset.AnimationClip.frameRate)); // 计算得到该动画的总帧数

            if (m_PlaybackTime >= m_AnimationAsset.AnimationClip.length - 0.001f) // 表示该动画已经播放到最后一秒了
            {
                m_Frame = totalFrames - 1; // 强制将帧数设置为最后一帧
            }
            else
            {
                m_Frame = Mathf.Clamp(AnimationTimeUtility.ToFrames(m_PlaybackTime, m_AnimationAsset.AnimationClip.frameRate), 0, totalFrames - 1);
            }

            if (m_RunningStatus == AnimationRunningStatus.Running && !m_IsInterrupting) // 如果当前动画正在播放中，且打断标识为设置为 true
            {
                if (m_HasInterruptRequest) Interrupt();
            }

            var wasInterrupting = m_IsInterrupting;
            EvaluateWeight();
            m_LayerMixer.SetInputWeight(1, m_Weight);

            // 只推送当前动画的通知，旧动画继续淡出时不再驱动通知
            var isComplete = m_RunningStatus == AnimationRunningStatus.Done;
            if (!wasInterrupting && m_OneShotAnimationParent.RunningAnimationState == this)
            {
                if (isComplete) m_Frame = totalFrames - 1;

                if (m_Frame != m_LastFrame || isComplete)
                {
                    info.output.PushNotification(playable,
                        new AnimationNotifyFrameData("AnimationNotify", m_Frame, m_LastFrame,
                            info.deltaTime, m_PlaybackTime, isComplete), this);
                }
            }

            m_LastFrame = m_Frame;

            if (m_RunningStatus == AnimationRunningStatus.Done)
            {
                playable.SetDone(true);
                m_IsDone = true;
                OnComplete();
            }

        }

        public void ForceInterruptAnimation(float easeOutDuration)
        {
            m_InterruptFadeOutDuration = (easeOutDuration < 0f ? m_AnimationAsset.EaseOutDuration : easeOutDuration) / m_PlaySpeed;
            Interrupt();
        }

        public bool InterruptAnimation(InterruptRequest request)
        {
            if (m_HasInterruptRequest) return false;
            if (request.InterruptPriority.Weight() < m_AnimationAsset.AnimationPriority.Weight()) return false;

            m_InterruptFadeOutDuration = (request.EaseOutDuration < 0f ? m_AnimationAsset.EaseOutDuration : request.EaseOutDuration) / m_PlaySpeed;
            m_HasInterruptRequest = true;

            return m_HasInterruptRequest;
        }

        public void BuildAnimationState(OneShotAnimationParent oneShotAnimationParent)
        {
            m_OneShotAnimationParent = oneShotAnimationParent;

            if (m_LayerMixer.IsValid()) m_LayerMixer.Destroy();
            m_LayerMixer = AnimationLayerMixerPlayable.Create(m_Graph, 2);
            if (m_AnimationClipPlayable.IsValid()) m_AnimationClipPlayable.Destroy();
            m_AnimationClipPlayable = AnimationClipPlayable.Create(m_Graph, m_AnimationAsset.AnimationClip);
            m_AnimationClipPlayable.SetApplyFootIK(true);

            if (m_AnimationAsset.UseAvatarMask)
            {
                m_LayerMixer.SetLayerMaskFromAvatarMask(1, m_AnimationAsset.AvatarMask);
                m_LayerMixer.SetLayerAdditive(1, m_AnimationAsset.AnimationMaskBlendMode == AnimationMaskBlendMode.Additive);
            }

            // Controller / 旧 State --> Mixer --> 本次 State --> Notify --> Parent
            var notifyPlayable = m_OneShotAnimationParent.ParentPlayable.GetInput(0);
            var upstreamPlayable = notifyPlayable.GetInput(0);

            // 断开原动画与 Parent 的连接
            notifyPlayable.DisconnectInput(0);

            // 将原动画和本次 Clip 接入 Mixer
            if (upstreamPlayable.IsValid())
                m_LayerMixer.ConnectInput(0, upstreamPlayable, 0);
            m_LayerMixer.ConnectInput(1, m_AnimationClipPlayable, 0);

            // 将混合结果经过本次 State，接回 Notify
            m_Self.ConnectInput(0, m_LayerMixer, 0);
            notifyPlayable.ConnectInput(0, m_Self, 0);

            // 保持传递链路有效；本次 Clip 从零权重开始淡入
            notifyPlayable.SetInputWeight(0, 1f);
            m_Self.SetInputWeight(0, 1f);
            m_LayerMixer.SetInputWeight(0, 1f);
            m_LayerMixer.SetInputWeight(1, 0f);
        }

        public bool MatchesAnimationName(string animationName)
        {
            return !string.IsNullOrEmpty(animationName) && string.Equals(animationName, m_AnimationAsset.AnimationName);
        }

        public bool MatchesAnimationTag(string animationTag)
        {
            return !string.IsNullOrEmpty(animationTag) && string.Equals(animationTag, m_AnimationAsset.AnimationTag);
        }

        public void SetSpeed(float speed)
        {
            if (speed <= 0f || float.IsNaN(speed) || float.IsInfinity(speed)) speed = 1f;

            m_PlaySpeed = speed;
            m_Duration = m_AnimationAsset.AnimationClip.length / m_PlaySpeed;

            // 同步更新 缓入、缓出 的时间
            UpdateTransitionTimes();

            m_Self.SetSpeed(m_PlaySpeed);
        }

        private void Init(PlayableGraph graph, AnimationAsset animationAsset)
        {
            m_Graph = graph;
            m_AnimationAsset = animationAsset;
            m_StartTime = Time.timeAsDouble;
            m_PlaySpeed = animationAsset.Speed;
            m_LastFrame = -1;

            SetSpeed(m_PlaySpeed);
        }

        private void Interrupt()
        {
            if (m_IsInterrupting || m_IsDone) return;

            m_InterruptTime = Time.timeAsDouble; // 记录一下打断的时间点
            m_IsInterrupting = true;
            m_OneShotAnimationParent.InterruptAnimationNotify(this);
        }

        private void OnComplete()
        {
            var downstreamPlayable = m_Self.GetOutput(0);
            var upstreamPlayable = m_LayerMixer.GetInput(0);

            downstreamPlayable.DisconnectInput(0);
            m_LayerMixer.DisconnectInput(0);

            if (upstreamPlayable.IsValid())
                downstreamPlayable.ConnectInput(0, upstreamPlayable, 0);
            downstreamPlayable.SetInputWeight(0, 1f);

            m_LayerMixer.DisconnectInput(1);
            m_Self.DisconnectInput(0);

            OnDestroy();
        }

        private void UpdateTransitionTimes()
        {
            var easeInDuration = Mathf.Max(0f, m_AnimationAsset.EaseInDuration) / m_PlaySpeed;
            var easeOutDuration = Mathf.Max(0f, m_AnimationAsset.EaseOutDuration) / m_PlaySpeed;

            // 缓入结束时间点
            m_EaseInEndTime = m_StartTime + easeInDuration;
            // 缓出开始时间点
            m_EaseOutStartTime = m_StartTime + (m_Duration - easeOutDuration);
        }

        private void EvaluateWeight()
        {
            if (m_IsDone) return;

            if (m_IsInterrupting) // 如果动画正在被打断过程中
            {
                var dx = Time.timeAsDouble - m_InterruptTime;
                var t = m_InterruptFadeOutDuration <= 0d ? 1f : (float)(dx / m_InterruptFadeOutDuration);

                if (t >= 1f)
                {
                    m_Weight = 0f;
                    m_IsInTransition = false;
                    m_RunningStatus = AnimationRunningStatus.Done;
                    m_IsInterrupting = false;
                    m_HasInterruptRequest = false;
                }
                else
                {
                    m_Weight = Mathf.Clamp01(m_AnimationAsset.EvaluateEaseOutWeight(t));
                    m_IsInTransition = true;
                    m_RunningStatus = AnimationRunningStatus.EaseOut;
                }
                return;
            }

            if (Time.timeAsDouble < m_EaseInEndTime) // 如果动画正在缓入过程中
            {
                var tIn = (float)((Time.timeAsDouble - m_StartTime) / (m_EaseInEndTime - m_StartTime));
                m_Weight = Mathf.Clamp01(m_AnimationAsset.EvaluateEaseInWeight(tIn));
                m_RunningStatus = AnimationRunningStatus.EaseIn;
                m_IsInTransition = true;
                return;
            }

            if (Time.timeAsDouble < m_EaseOutStartTime) // 动画正常播放
            {
                m_Weight = 1f;
                m_RunningStatus = AnimationRunningStatus.Running;
                m_IsInTransition = false;
                return;
            }

            // 如果动画正在缓出过程中
            var endTime = m_StartTime + m_Duration;
            if (Time.timeAsDouble >= endTime)
            {
                m_Weight = 0f;
                m_RunningStatus = AnimationRunningStatus.Done;
                m_IsInTransition = false;
                return;
            }
            // 假设总时长是4s，进入缓出时间点3s
            var tOut = (float)((Time.timeAsDouble - m_EaseOutStartTime) / (endTime - m_EaseOutStartTime));
            m_Weight = Mathf.Clamp01(m_AnimationAsset.EvaluateEaseOutWeight(tOut));
            m_RunningStatus = GMathf.Approximately(m_Weight, 0f) ? AnimationRunningStatus.Done : AnimationRunningStatus.EaseOut;
            m_IsInTransition = true;
        }
    }
}
