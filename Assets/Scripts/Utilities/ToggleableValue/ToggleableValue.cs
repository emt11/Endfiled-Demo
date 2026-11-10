using UnityEngine;
using System;
using Sirenix.OdinInspector;

namespace GP
{
    [Serializable]
    public class ToggleableValue<T> where T : struct
    {
        [SerializeField] private bool m_IsEnabled;
        [SerializeField, ShowIfGroup(nameof(m_IsEnabled))] private T m_Value;

        public bool IsEnabled => m_IsEnabled;
        public T Value => m_IsEnabled ? m_Value : default;
    }
}
