using System;

namespace GP
{
    /// <summary>管理战斗模式；连招中的换招由控制器重建攻击上下文，不重入 Attack 状态</summary>
    public sealed class MeleeStateMachine
    {
        private readonly MeleeIdleState m_IdleState;
        private readonly MeleeAttackState m_AttackState;
        private readonly MeleeActionState m_ActionState;

        public MeleeCombatController CombatController { get; }
        public IMeleeState CurrentState { get; private set; }
        public CombatStatus Status => CurrentState?.Status ?? CombatStatus.Idle;

        public MeleeStateMachine(MeleeCombatController combatController)
        {
            CombatController = combatController ? combatController : throw new ArgumentNullException(nameof(combatController));
            m_IdleState = new MeleeIdleState(this);
            m_AttackState = new MeleeAttackState(this);
            m_ActionState = new MeleeActionState(this);
            EnterIdle();
        }

        public void EnterIdle() => ChangeState(m_IdleState);

        public void EnterAttack()
        {
            if (Status != CombatStatus.Action && CombatController.IsAttackAnimationRunning)
                ChangeState(m_AttackState);
        }

        public void EnterAction() => ChangeState(m_ActionState);

        public void Tick() => CurrentState?.Tick();

        public void Clear()
        {
            var previousState = CurrentState;
            CurrentState = null;
            previousState?.Exit();
        }

        private void ChangeState(IMeleeState state)
        {
            if (ReferenceEquals(CurrentState, state)) return;
            var previousState = CurrentState;
            // 先发布目标状态，旧动画退出时产生的通知不再被当作攻击通知接收
            CurrentState = state;
            previousState?.Exit();
            if (ReferenceEquals(CurrentState, state)) state.Enter();
        }
    }
}
