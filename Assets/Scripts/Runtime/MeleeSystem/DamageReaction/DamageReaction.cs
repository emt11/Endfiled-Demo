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

        public AnimationAsset Animation => m_Animation;

        public bool Matches(HitDirection direction, int damage)
        {
            return direction == m_HitDirection && (!m_UseMinimumDamage || damage >= m_MinimumDamage);
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
        RightBackward = 10
    }
}
