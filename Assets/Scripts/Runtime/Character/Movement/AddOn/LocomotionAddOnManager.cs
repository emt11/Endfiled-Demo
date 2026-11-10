using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GP
{
    [Serializable]
    public class LocomotionAddOnManager
    {
        [SerializeField, BoxGroup("Locomotion", showLabel: false), LabelText("游泳运动模式")]
        private SwimmingLocomotion m_Swimming = new();

        [SerializeField, BoxGroup("Locomotion", showLabel: false), LabelText("滑索运动模式")]
        private ZiplineLocomotion m_Zipline = new();
        private ThirdPersonLocomotion m_Locomotion;

        private ILocomotionAddOn m_DefaultLocomotion;

        public ILocomotionAddOn RunningLocomotion { get; private set; }
        public LocomotionMode LocomotionMode => RunningLocomotion?.LocomotionMode ?? LocomotionMode.None;
        public ThirdPersonLocomotion Locomotion => m_Locomotion;

        public event Action<LocomotionMode> LocomotionModeChanged;

        public void OnDestroy()
        {
            m_DefaultLocomotion.LocomotionModeChanged -= HandleLocomotionModeChanged;
            m_Swimming.LocomotionModeChanged -= HandleLocomotionModeChanged;
            m_Zipline.LocomotionModeChanged -= HandleLocomotionModeChanged;

            m_DefaultLocomotion?.OnDestroy();
            m_Swimming?.OnDestroy();
            m_Zipline?.OnDestroy();

            LocomotionModeChanged = null;
        }

        public void Initialize(ThirdPersonLocomotion locomotion)
        {
            m_Locomotion = locomotion;

            m_DefaultLocomotion = new DefaultLocomotion();

            m_DefaultLocomotion.Initialize(this);
            m_Swimming?.Initialize(this);
            m_Zipline?.Initialize(this);

            m_DefaultLocomotion.LocomotionModeChanged += HandleLocomotionModeChanged;
            m_Swimming.LocomotionModeChanged += HandleLocomotionModeChanged;
            m_Zipline.LocomotionModeChanged += HandleLocomotionModeChanged;

            RunningLocomotion?.CutOutLocomotion();
            RunningLocomotion = m_DefaultLocomotion;
            RunningLocomotion?.CutInLocomotion();
        }

        public void SwitchLocomotion(LocomotionMode mode)
        {
            if (RunningLocomotion?.LocomotionMode == mode) return;

            RunningLocomotion?.CutOutLocomotion();

            RunningLocomotion = mode switch
            {
                LocomotionMode.WaterLocomotion => m_Swimming,
                LocomotionMode.ZiplineLocomotion => m_Zipline,
                _ => m_DefaultLocomotion
            };

            RunningLocomotion?.CutInLocomotion();
        }

        private void HandleLocomotionModeChanged(LocomotionMode mode)
        {
            LocomotionModeChanged?.Invoke(mode);
        }
    }
}
