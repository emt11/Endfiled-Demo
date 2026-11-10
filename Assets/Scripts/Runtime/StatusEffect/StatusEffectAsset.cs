using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace GP
{
    [CreateAssetMenu(fileName = "New StatusEffectAsset", menuName = "GP/StatusEffect/StatusEffectAsset")]
    public class StatusEffectAsset : ScriptableObject
    {
        [SerializeField, BoxGroup("Info", showLabel: false)]
        private string m_StatusEffectName;
        [SerializeField, BoxGroup("Info", showLabel: false)]
        private StatusEffectType m_StatusEffectType;
        [SerializeField, Min(0), BoxGroup("Info", showLabel: false)]
        private int m_StatusEffectValue;

        [SerializeField, BoxGroup("Settings", showLabel: false), LabelText("效果可被叠加")]
        private bool m_IsStackable;
        [SerializeField, BoxGroup("Settings", showLabel: false), ShowIf(nameof(m_IsStackable)), LabelText("效果层值")]
        private StatusEffectStackValue m_StackValue = StatusEffectStackValue.Line;
        [SerializeField, Min(1), BoxGroup("Settings", showLabel: false), ShowIf(nameof(m_IsStackable)), LabelText("效果最大叠加层")]
        private int m_StackMaxCount = 1;

        [SerializeField, BoxGroup("Settings", showLabel: false), LabelText("效果是否持续性")]
        [FormerlySerializedAs("m_IsDurationable")]
        private bool m_HasDuration;
        [SerializeField, Min(0f), BoxGroup("Settings", showLabel: false), ShowIf(nameof(m_HasDuration)), LabelText("效果持续时间")]
        private float m_Duration = 1f;
        [SerializeField, Min(0f), BoxGroup("Settings", showLabel: false), ShowIf(nameof(m_HasDuration)), LabelText("间隔时间")]
        [Tooltip("大于 0 时按间隔重复执行；为 0 时仅在开始执行一次，持续时间结束后退出。")]
        private float m_TickInterval = 1f;

        [SerializeReference, BoxGroup("Effect Strategies", showLabel: false), LabelText("效果策略")]
        private List<IStatusEffectStrategy> m_StatusEffectStrategies = new List<IStatusEffectStrategy>();

        public int StatusEffectId => string.IsNullOrEmpty(m_StatusEffectName) ? -1 : m_StatusEffectName.GetHashCode();
        public string StatusEffectName => m_StatusEffectName;
        public StatusEffectType StatusEffectType => m_StatusEffectType;
        public int StatusEffectValue => Math.Max(0, m_StatusEffectValue);

        public bool IsStackable => m_IsStackable;
        public StatusEffectStackValue StackValue => m_StackValue;
        public int StackMaxCount => m_IsStackable ? Math.Max(1, m_StackMaxCount) : 1;
        public bool HasDuration => m_HasDuration;
        public float Duration => m_HasDuration ? Math.Max(0f, m_Duration) : 0f;
        public float TickInterval => Math.Max(0f, m_TickInterval);
        public IReadOnlyList<IStatusEffectStrategy> StatusEffectStrategies => m_StatusEffectStrategies;

        public int GetStatusEffectValue(int stackCount)
        {
            return GetStatusEffectValue(m_StackValue, stackCount);
        }

        public int GetStatusEffectValue(StatusEffectStackValue stackValue, int stackCount)
        {
            return CalculateEffectValue(StatusEffectValue, stackValue, Mathf.Clamp(stackCount, 0, StackMaxCount));
        }

        internal static int CalculateEffectValue(int baseValue, StatusEffectStackValue stackValue, int stackCount)
        {
            if (stackCount <= 0) return 0;

            switch (stackValue)
            {
                case StatusEffectStackValue.Single:
                    return baseValue;
                case StatusEffectStackValue.Line:
                    long value = (long)baseValue * stackCount;
                    return (int)Math.Max(int.MinValue, Math.Min(value, int.MaxValue));
                default:
                    throw new ArgumentOutOfRangeException(nameof(stackValue), stackValue, "不支持的效果叠加方式。");
            }
        }
    }

    public enum StatusEffectType
    {
        None,
        Gain,
        Penalty
    }

    public enum StatusEffectStackValue
    {
        [InspectorName("单层")] Single,
        [InspectorName("线性叠加")] Line
    }
}
