using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [CreateAssetMenu(fileName = "New-AnimationAsset", menuName = "GP/Animation/AnimationAsset")]
    public class AnimationAsset : ScriptableObject
    {
        [Space, SerializeField, BoxGroup("BaseSet", showLabel: false), Required("眼睛往这看,这里不能为空!")]
        private AnimationClip m_AnimationClip;
        [SerializeField, BoxGroup("BaseSet", showLabel: false)]
        private float m_Speed = 1f;
        [SerializeField, BoxGroup("BaseSet", showLabel: false)]
        private bool m_UseRootMotion = true;
        [SerializeField, BoxGroup("BaseSet", showLabel: false)]
        private bool m_UseAvatarMask;
        [SerializeField, BoxGroup("BaseSet", showLabel: false), ShowIf(nameof(m_UseAvatarMask))]
        private AvatarMask m_AvatarMask;
        [SerializeField, BoxGroup("BaseSet", showLabel: false), ShowIf(nameof(m_UseAvatarMask))]
        private AnimationMaskBlendMode m_AnimationMaskBlendMode;

        [SerializeField, BoxGroup("NameSet", showLabel: false), HorizontalGroup("NameSet/S", MarginRight = .15f), LabelText("动画名称")]
        private string m_AnimationName;
        [SerializeField, BoxGroup("NameSet", showLabel: false), HorizontalGroup("NameSet/S"), LabelText("动画标签")]
        private string m_AnimationTag;

        [SerializeField, BoxGroup("Priority", showLabel: false)]
        private Priority m_AnimationPriority = Priority.Normal;

        [Space, SerializeField, BoxGroup("Notify", showLabel: false), TitleGroup("Notify/🔈动画通知", alignment: TitleAlignments.Right), LabelText("📡AnimationNotify")]
        private AnimationNotifyList m_Notifies = new();

        [Space, SerializeField, BoxGroup("Notify", showLabel: false), TitleGroup("Notify/🔈动画通知", alignment: TitleAlignments.Right), LabelText("📡AnimationStateNotify")]
        private AnimationStateNotifyList m_StateNotifies = new();

        [Space, SerializeField, BoxGroup("AnimationEaseSet", showLabel: false), Range(0.1f, 1f)]
        private float m_EaseInDuration = .15f;
        [SerializeField, BoxGroup("AnimationEaseSet", showLabel: false), Range(0.1f, 1f)]
        private float m_EaseOutDuration = .15f;
        [Space, SerializeField, BoxGroup("AnimationEaseSet", showLabel: false)]
        private AnimationCurve m_AnimationEaseIn = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
        [SerializeField, BoxGroup("AnimationEaseSet", showLabel: false)]
        private AnimationCurve m_AnimationEaseOut = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));

        [Space, SerializeField, BoxGroup("SyncAnimation", showLabel: false)]
        private bool m_SyncAnimation;
        [Space, SerializeField, BoxGroup("SyncAnimation", showLabel: false), ShowIf(nameof(m_SyncAnimation))]
        private AnimationAsset m_SyncAnimationAsset;

        [Space, SerializeField, BoxGroup("Warp", showLabel: false)]
        private bool m_UseAnimationWarp;
        [Space, SerializeField, BoxGroup("Warp", showLabel: false), ShowIf(nameof(m_UseAnimationWarp))]
        private MotionWarpingAsset m_WarpAsset;

        public AnimationClip AnimationClip => m_AnimationClip;
        public float Speed => m_Speed;
        public bool UseRootMotion => m_UseRootMotion;
        public bool UseAvatarMask => m_UseAvatarMask;
        public AvatarMask AvatarMask => m_AvatarMask;
        public AnimationMaskBlendMode AnimationMaskBlendMode => m_AnimationMaskBlendMode;
        public float EaseInDuration => m_EaseInDuration;
        public float EaseOutDuration => m_EaseOutDuration;
        public string AnimationName => m_AnimationName;
        public string AnimationTag => m_AnimationTag;
        public Priority AnimationPriority => m_AnimationPriority;
        public AnimationNotifyList NotifyList => m_Notifies;
        public AnimationStateNotifyList StateNotifyList => m_StateNotifies;
        public bool HasNotify => m_Notifies?.IsValid == true || m_StateNotifies?.IsValid == true;
        public bool UseAnimationWarp => m_UseAnimationWarp;
        public bool SyncAnimation => m_SyncAnimation;
        public AnimationAsset SyncAnimationAsset => m_SyncAnimationAsset;
        public MotionWarpingAsset WarpAsset => m_WarpAsset;

        public float EvaluateEaseInWeight(float progress) => m_AnimationEaseIn.Evaluate(progress);
        public float EvaluateEaseOutWeight(float progress) => m_AnimationEaseOut.Evaluate(progress);

        public void PlaySyncAnimation(AnimationSource targetAnimationSource)
        {
            if (!SyncAnimation) return;

            targetAnimationSource?.PlayAnimation(m_SyncAnimationAsset);
        }
    }

    public enum AnimationMaskBlendMode
    {
        Additive, // 叠加
        Override // 覆盖
    }

    [Serializable]
    public class AnimationNotifyList
    {
        [SerializeReference] private List<AnimationNotify> m_Notifies = new List<AnimationNotify>();

        public bool IsValid => m_Notifies != null && m_Notifies.Exists(notify => notify != null);

        // 排序交给 RuntimeWrapperManager，避免读取时改动共享资源中的列表
        public IReadOnlyList<AnimationNotify> Notifies => m_Notifies;
    }

    [Serializable]
    public class AnimationStateNotifyList
    {
        [SerializeReference] private List<AnimationStateNotify> m_StateNotifies = new List<AnimationStateNotify>();

        public bool IsValid => m_StateNotifies != null && m_StateNotifies.Exists(notify => notify != null);

        public IReadOnlyList<AnimationStateNotify> StateNotifies => m_StateNotifies;
    }
}
