using System;

namespace GP
{
    public interface ILocomotionAddOn
    {
        public LocomotionMode LocomotionMode { get; }

        event Action<LocomotionMode> LocomotionModeChanged;

        void Initialize(LocomotionAddOnManager locomotionManager);
        void CutInLocomotion();
        void CutOutLocomotion();
        void LocomotionAddOnTick();
        void OnDrawGizmos();
        void OnDestroy();
    }

    public enum LocomotionMode
    {
        None,
        WaterLocomotion,
        ZiplineLocomotion
    }
}
