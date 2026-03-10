namespace Monster.StateMachine.States
{
    /// <summary>
    /// 死亡状態。終端状態のため遷移不可。
    /// CharacterControl 側の DeadState と連動する。
    /// </summary>
    public class EnemyDeadState : EnemyStateBase
    {
        public override bool CanBeInterruptedBy(IEnemyState newState) => false;

        protected override void OnEnter()
        {
            StopMove();
            SetAttack(false);
            SetGuard(false);
        }
    }
}
