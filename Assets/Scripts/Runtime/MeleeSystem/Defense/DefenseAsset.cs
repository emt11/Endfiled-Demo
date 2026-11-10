using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [CreateAssetMenu(fileName = "New Defense", menuName = "GP/Melee System/Defense")]
    public class DefenseAsset : ScriptableObject
    {
        [SerializeField, Min(0f), LabelText("完美格挡窗口（秒）")]
        private float m_PerfectParryDuration = 0.2f;
        [SerializeField, Min(0f), LabelText("防御冷却（秒）")]
        private float m_Cooldown = 0.5f;
        [SerializeField, Range(0f, 360f), LabelText("正面防御角度")]
        private float m_BlockAngle = 180f;
        [SerializeField, Min(0f), LabelText("伤害转体力消耗系数")]
        private float m_StaminaCostMultiplier = 0.5f;
        [SerializeField, LabelText("普通格挡")]
        private DefenseAction m_Blocked = new DefenseAction();
        [SerializeField, LabelText("完美格挡")]
        private DefenseAction m_PerfectParry = new DefenseAction();
        [SerializeField, LabelText("破防")]
        private DefenseAction m_GuardBreak = new DefenseAction();

        public float PerfectParryDuration => m_PerfectParryDuration;
        public float Cooldown => m_Cooldown;
        public float BlockAngle => m_BlockAngle;
        public float StaminaCostMultiplier => m_StaminaCostMultiplier;

        public DefenseAction GetAction(DefensePosture posture)
        {
            return posture switch
            {
                DefensePosture.Blocked => m_Blocked,
                DefensePosture.PerfectParry => m_PerfectParry,
                DefensePosture.GuardBreak => m_GuardBreak,
                _ => null
            };
        }
    }

    [Serializable]
    public class DefenseAction
    {
        [SerializeField, LabelText("反应动作")]
        private DamageReactionAsset m_Reactions;
        [SerializeField, LabelText("音效")]
        private AudioAsset m_Sound;
        [SerializeField, LabelText("特效")]
        private FXAsset m_Effect;

        public DamageReactionResult FindReaction(HitDirection direction, int damage)
        {
            return m_Reactions ? m_Reactions.FindReactionResult(direction, damage) : default;
        }

        public void PlayFeedback(Vector3 point, Quaternion rotation)
        {
            // 格挡成立就播放反馈，不依赖是否另外配置了反应动画
            if (m_Sound) m_Sound.PlaySoundWithPool(point, rotation);
            if (m_Effect) m_Effect.Play(point, rotation);
        }
    }
}
