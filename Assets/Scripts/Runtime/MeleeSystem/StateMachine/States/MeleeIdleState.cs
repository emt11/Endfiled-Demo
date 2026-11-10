namespace GP
{
    public sealed class MeleeIdleState : IMeleeState
    {
        private readonly MeleeStateMachine m_StateMachine;

        public CombatStatus Status => CombatStatus.Idle;

        public MeleeIdleState(MeleeStateMachine stateMachine) => m_StateMachine = stateMachine;

        public void Enter() { }
        public void Tick() => m_StateMachine.CombatController.ProcessBufferedInput();
        public void Exit() { }
    }
}
