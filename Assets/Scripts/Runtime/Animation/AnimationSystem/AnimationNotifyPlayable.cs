using UnityEngine;
using UnityEngine.Playables;

namespace GP
{
    public class AnimationNotifyPlayable : PlayableBehaviour, INotificationReceiver
    {
        private GameObject m_AnimationTarget;
        private IAnimationState m_SourceAnimationState;
        private RuntimeWrapperManager m_WrapperManager;

        public bool HasNotifies => m_WrapperManager != null;

        public static ScriptPlayable<AnimationNotifyPlayable> Create(PlayableGraph graph, GameObject animationTarget)
        {
            var handle = ScriptPlayable<AnimationNotifyPlayable>.Create(graph, 1);
            handle.GetBehaviour().m_AnimationTarget = animationTarget;
            return handle;
        }

        public void SetAnimationNotifies(AnimationAsset asset, IAnimationState sourceAnimationState = null)
        {
            ClearAnimationNotifies();
            if (asset == null || !asset.HasNotify) return;

            m_SourceAnimationState = sourceAnimationState;
            m_WrapperManager = new RuntimeWrapperManager(asset.NotifyList, asset.StateNotifyList, m_AnimationTarget);
        }

        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (!(notification is AnimationNotifyFrameData data)) return;
            // 淡出的旧动画和已经排队的旧通知不能推进新动画的通知游标
            if (m_SourceAnimationState != null && !ReferenceEquals(context, m_SourceAnimationState)) return;

            var manager = m_WrapperManager;
            manager?.EvaluateFrame(data.CurrentFrame);

            // PushNotification 延迟派发，必须先消费末帧通知，再清理状态
            if (data.IsAnimationComplete && ReferenceEquals(manager, m_WrapperManager))
                ClearAnimationNotifies();
        }

        public void InterruptNotifies()
        {
            ClearAnimationNotifies();
        }

        public void ClearAnimationNotifies()
        {
            var manager = m_WrapperManager;
            m_WrapperManager = null;
            m_SourceAnimationState = null;
            manager?.InterruptNotifies();
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            ClearAnimationNotifies();
        }
    }

    public readonly struct AnimationNotifyFrameData : INotification
    {
        public readonly int CurrentFrame;
        public readonly int PreviousFrame;
        public readonly float DeltaTime;
        public readonly float PlaybackTime;
        public readonly bool IsAnimationComplete;

        public PropertyName id { get; }

        public AnimationNotifyFrameData(string notifyName, int currentFrame, int previousFrame,
            float deltaTime, float playbackTime, bool isAnimationComplete = false)
        {
            id = new PropertyName(notifyName);
            CurrentFrame = currentFrame;
            PreviousFrame = previousFrame;
            DeltaTime = deltaTime;
            PlaybackTime = playbackTime;
            IsAnimationComplete = isAnimationComplete;
        }
    }
}
