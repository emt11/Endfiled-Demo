namespace GP
{
    /// <summary>信号和触发方式一起传递，避免消费缓存时取到另一种输入。</summary>
    public readonly struct MeleeInputCommand
    {
        public MeleeInputTrigger Trigger { get; }
        public MeleeInputSignal Signal { get; }
        public bool IsValid => (Signal == MeleeInputSignal.LeftAttack || Signal == MeleeInputSignal.RightAttack)
            && (Trigger == MeleeInputTrigger.Tap || Trigger == MeleeInputTrigger.Charge);

        public MeleeInputCommand(MeleeInputTrigger trigger, MeleeInputSignal signal)
        {
            Trigger = trigger;
            Signal = signal;
        }
    }
}
