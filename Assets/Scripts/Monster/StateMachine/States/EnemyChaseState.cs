namespace Monster.StateMachine.States
{
    /// <summary>
    /// 追跡状態。ターゲットへ向かって移動し、攻撃範囲内に入ったら AttackState へ移行する。
    /// ターゲットが lostRange 以上離れたら Idle へ戻る。
    /// </summary>
    public class EnemyChaseState : EnemyStateBase
    {
        protected override void OnEnter()
        {
            SetAttack(false);
            SetGuard(false);
        }

        protected override void OnUpdate()
        {
            if (Control.GetIsDead()) { ForceChangeState<EnemyDeadState>(); return; }
            if (ShouldFlee()) { ChangeState<EnemyFleeState>(); return; }

            float dist = DistanceToTarget();

            if (Target == null || Target.GetIsDead() || dist > AIData.lostRange)
            {
                ChangeState<EnemyIdleState>();
                return;
            }

            if (IsTargetInAttackRange())
            {
                ChangeState<EnemyAttackState>();
                return;
            }

            // ターゲットへ向かって移動
            SetMove(DirectionToTarget() * AIData.moveSpeed * Machine.SpeedMultiplier);
        }

        protected override void OnExit()
        {
            StopMove();
        }
    }
}
