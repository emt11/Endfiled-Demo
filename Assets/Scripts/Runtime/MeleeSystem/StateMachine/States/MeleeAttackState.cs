namespace GP
{
    public sealed class MeleeAttackState : IMeleeState
    {
        private readonly MeleeStateMachine m_StateMachine;

        public CombatStatus Status => CombatStatus.Attacking;

        public MeleeAttackState(MeleeStateMachine stateMachine) => m_StateMachine = stateMachine;

        public void Enter() { }

        public void Tick()
        {
            var controller = m_StateMachine.CombatController;
            if (!controller.IsAttackAnimationRunning)
            {
                m_StateMachine.EnterIdle();
                return;
            }

            // 打击窗口和输入窗口可以重叠，不用一个阶段枚举互相覆盖
            controller.ProcessHitboxes();
            // 命中回调可能已经取消攻击或切入特殊动作
            if (ReferenceEquals(m_StateMachine.CurrentState, this)
                && controller.IsAttackAnimationRunning && controller.IsInputWindowOpen)
                controller.ProcessBufferedInput();
        }

        public void Exit() => m_StateMachine.CombatController.EndAttack();
    }
}
