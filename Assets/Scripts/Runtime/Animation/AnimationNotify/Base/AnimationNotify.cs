using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [Serializable]
    public abstract class AnimationNotify
    {
        [SerializeField, BoxGroup("Notify", showLabel: false)]
        private int m_TriggerFrame; // 通知的触发帧

        public int TriggerFrame => m_TriggerFrame;

        public abstract void TriggerNotify(GameObject user);
    }
}
