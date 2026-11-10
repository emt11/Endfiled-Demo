using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [Serializable]
    public class DamageReaction
    {
        [SerializeField, LabelText("打击方向")]
        private HitDirection m_HitDirection;
        [SerializeField, LabelText("限制最低伤害")]
        private bool m_UseMinimumDamage;
        [SerializeField, Min(0f), ShowIf(nameof(m_UseMinimumDamage))]
        private float m_MinimumDamage;
        [SerializeField, LabelText("受击动画")]
        private AnimationAsset m_Animation;
        [SerializeField, LabelText("允许提前取消")]
        private bool m_AllowCancel;
        [SerializeField, Min(0f), ShowIf(nameof(m_AllowCancel)), LabelText("可取消时间（秒）")]
        private float m_CancelAfter = 0.5f;

        public AnimationAsset Animation => m_Animation;
        public DamageReactionResult Result => new DamageReactionResult(m_Animation, m_AllowCancel ? m_CancelAfter : -1f);

        public bool Matches(HitDirection direction, int damage)
        {
            return (m_HitDirection == HitDirection.Any || direction == m_HitDirection)
                && (!m_UseMinimumDamage || damage >= m_MinimumDamage);
        }
    }

    public readonly struct DamageReactionResult
    {
        public readonly AnimationAsset Animation;
        public readonly float CancelAfter;

        public bool IsValid => Animation && Animation.AnimationClip;

        public DamageReactionResult(AnimationAsset animation, float cancelAfter = -1f)
        {
            Animation = animation;
            CancelAfter = cancelAfter;
        }
    }
    public enum HitDirection
    {
        None = 0,
        Forward = 1,
        Backward = 2,
        Left = 3,
        Right = 4,
        Up = 5,
        Down = 6,
        LeftForward = 7,
        LeftBackward = 8,
        RightForward = 9,
        RightBackward = 10,
        Any = 11
    }
}
