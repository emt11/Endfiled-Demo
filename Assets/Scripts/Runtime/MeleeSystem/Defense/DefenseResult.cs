namespace GP
{
    public enum DefensePosture
    {
        None,
        Blocked,
        PerfectParry,
        GuardBreak
    }

    public readonly struct DefenseResult
    {
        public readonly DefensePosture Posture;
        public readonly int HealthDamage;
        public readonly int StaminaCost;

        public DefenseResult(DefensePosture posture, int healthDamage, int staminaCost = 0)
        {
            Posture = posture;
            HealthDamage = healthDamage;
            StaminaCost = staminaCost;
        }
    }
}
