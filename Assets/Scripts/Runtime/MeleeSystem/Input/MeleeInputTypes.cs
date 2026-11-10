using UnityEngine;

namespace GP
{
    public enum MeleeInputSignal
    {
        None = 0,
        LeftAttack = 1,
        RightAttack = 2
    }
    public enum MeleeInputTrigger
    {
        [InspectorName("蓄力（短按时触发 Tap）")] Charge = 0,
        [InspectorName("单击")] Tap = 1
    }
}
