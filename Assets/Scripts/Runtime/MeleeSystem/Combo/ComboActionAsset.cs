using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [CreateAssetMenu(fileName = "New Combo Action", menuName = "GP/Melee System/Combo Action")]
    public class ComboActionAsset : ScriptableObject
    {
        [SerializeField, LabelText("动作名称")]
        private string m_ActionName;
        [SerializeField, LabelText("动画资产")]
        private AnimationAsset m_AnimationAsset;
        [SerializeField, LabelText("触发方式")]
        private MeleeInputTrigger m_InputTrigger = MeleeInputTrigger.Tap;
        [SerializeField, LabelText("输入信号")]
        private MeleeInputSignal m_InputSignal;
        [SerializeField, LabelText("允许的检测部位")]
        private List<WeaponAttachmentPoint> m_HitboxPoints = new List<WeaponAttachmentPoint>();
        [SerializeField, Min(0), LabelText("伤害值")]
        private int m_Damage = 10;

        public string ActionName => m_ActionName;
        public AnimationAsset Animation => m_AnimationAsset;
        public MeleeInputTrigger InputTrigger => m_InputTrigger;
        public MeleeInputSignal InputSignal => m_InputSignal;
        public int Damage => Mathf.Max(0, m_Damage);

        public bool MatchesInput(MeleeInputTrigger trigger, MeleeInputSignal signal)
        {
            return signal != MeleeInputSignal.None && m_InputTrigger == trigger && m_InputSignal == signal;
        }

        public WeaponAttachmentPoint GetAllowedHitboxPoints(WeaponAttachmentPoint requestedPoints)
        {
            var allowedPoints = WeaponAttachmentPoint.None;
            if (m_HitboxPoints == null) return allowedPoints;
            foreach (var point in m_HitboxPoints) allowedPoints |= point;
            return requestedPoints & allowedPoints & WeaponAttachmentPoint.All;
        }
    }
}
