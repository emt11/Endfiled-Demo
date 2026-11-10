using System;

namespace GP
{
    public class DefaultLocomotion : ILocomotionAddOn
    {
        private readonly LocomotionMode m_LocomotionMode = LocomotionMode.None;
        private LocomotionAddOnManager m_LocomotionManager;
        private ThirdPersonLocomotion m_Locomotion;

        public LocomotionMode LocomotionMode => m_LocomotionMode;
        public event Action<LocomotionMode> LocomotionModeChanged;

        public void OnDestroy()
        {
            LocomotionModeChanged = null;
        }

        public void Initialize(LocomotionAddOnManager locomotionAddOnManager)
        {
            m_LocomotionManager = locomotionAddOnManager;
            m_Locomotion = locomotionAddOnManager.Locomotion;
        }

        public void CutInLocomotion()
        {
            m_Locomotion?.SetInputFrozen(false);
            m_Locomotion?.SetGroundCheckEnabled(true);
            m_Locomotion?.SetGravityEnabled(true);
            LocomotionModeChanged?.Invoke(m_LocomotionMode);
        }

        public void CutOutLocomotion()
        {

        }

        public void LocomotionAddOnTick()
        {

        }

        public void OnDrawGizmos()
        {

        }
    }
}
