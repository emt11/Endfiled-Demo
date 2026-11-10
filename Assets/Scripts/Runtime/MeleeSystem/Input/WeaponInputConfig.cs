using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    /// <summary>按钮的输入识别配置；Charge 模式同时支持短按和蓄力</summary>
    [Serializable]
    public class WeaponInputConfig
    {
        [SerializeField, LabelText("识别模式")]
        private MeleeInputTrigger m_Trigger = MeleeInputTrigger.Tap;
        [SerializeField, LabelText("输入信号（None 禁用）")]
        private MeleeInputSignal m_InputSignal = MeleeInputSignal.None;
        [SerializeField, ShowIf(nameof(m_Trigger), MeleeInputTrigger.Charge)]
        private bool m_AutoTrigger;
        [SerializeField, Min(0f), ShowIf(nameof(m_Trigger), MeleeInputTrigger.Charge)]
        private float m_HoldDuration = 0.5f;

        public MeleeInputTrigger Mode => m_Trigger;
        public MeleeInputSignal InputSignal => m_InputSignal;
        public bool AutoTrigger => m_AutoTrigger;
        public float HoldDuration => float.IsNaN(m_HoldDuration) || float.IsInfinity(m_HoldDuration)
            ? 0.5f : Mathf.Max(0f, m_HoldDuration);
    }
}
