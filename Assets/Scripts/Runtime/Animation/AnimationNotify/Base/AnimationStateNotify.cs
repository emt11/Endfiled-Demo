using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [Serializable]
    public abstract class AnimationStateNotify
    {
        [SerializeField, BoxGroup("StateNotify", showLabel: false)]
        private int m_StartFrame;
        [SerializeField, BoxGroup("StateNotify", showLabel: false)]
        private int m_EndFrame;

        public int StartFrame => m_StartFrame;
        public int EndFrame => m_EndFrame;

        public abstract void OnEnterNotify(GameObject user);
        public abstract void OnTickNotify(GameObject user);
        public abstract void OnExitNotify(GameObject user);
    }
}
