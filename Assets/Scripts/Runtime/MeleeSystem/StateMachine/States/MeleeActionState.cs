namespace GP
{
    /// <summary>为外部特殊动作预留的状态；结束时由调用者切回空闲</summary>
    public sealed class MeleeActionState : IMeleeState
    {
        public CombatStatus Status => CombatStatus.Action;

        public void Enter() { }
        public void Tick() { }
        public void Exit() { }
    }
}
