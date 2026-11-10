namespace GP
{
    /// <summary>管理受击反应；外部手动激活的动作仍由调用者结束</summary>
    public sealed class MeleeActionState : IMeleeState
    {
        private readonly MeleeStateMachine m_StateMachine;

        public CombatStatus Status => CombatStatus.Action;

        public MeleeActionState(MeleeStateMachine stateMachine) => m_StateMachine = stateMachine;

        public void Enter() { }
        public void Tick() => m_StateMachine.CombatController.UpdateReaction();
        public void Exit() => m_StateMachine.CombatController.EndReaction();
    }
}
