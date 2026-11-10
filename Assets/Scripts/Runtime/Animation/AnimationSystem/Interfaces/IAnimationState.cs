

namespace GP
{
    public interface IAnimationState
    {
        float PlaybackTime { get; } // 当前播放时间
        float Duration { get; } // 动画时长
        float NormalizedTime { get; } // 归一化播放进度
        float Weight { get; } // 当前动画的混合权重

        AnimationRunningStatus RunningStatus { get; } // 动画播放状态

        string AnimationName { get; }
        string AnimationTag { get; }

        bool IsInTransition { get; } // 是否处于过渡阶段
        bool IsDone { get; } // 是否播放完毕

        int Frame { get; } // 动画当前帧

        bool UseRootMotion { get; }

        void SetSpeed(float speed);
        bool InterruptAnimation(InterruptRequest request);
        void ForceInterruptAnimation(float easeOutDuration);
        void OnDestroy();

        bool MatchesAnimationName(string animationName);
        bool MatchesAnimationTag(string animationTag);
    }

    public enum AnimationRunningStatus
    {
        EaseIn,
        Running,
        EaseOut,
        Done
    }

    public readonly struct InterruptRequest
    {
        public readonly Priority InterruptPriority;
        public readonly float EaseOutDuration;

        public static InterruptRequest None => new InterruptRequest(Priority.None, -1f);

        public InterruptRequest(Priority interruptPriority, float easeOutDuration)
        {
            InterruptPriority = interruptPriority;
            EaseOutDuration = easeOutDuration;
        }

        public bool IsValid() => InterruptPriority != Priority.None && EaseOutDuration > 0f;
    }
}
