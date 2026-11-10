namespace GP
{
    public interface IMeleeState
    {
        CombatStatus Status { get; }
        void Enter();
        void Tick();
        void Exit();
    }
}
