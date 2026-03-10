using UnityEngine;

namespace Monster.StateMachine.States
{
    /// <summary>
    /// クールダウン状態。攻撃後の待機時間を管理する。
    /// 待機完了後、攻撃範囲内なら Attack、外なら Chase へ移行する。
    /// </summary>
    public class EnemyCooldownState : EnemyStateBase
    {
        private float _timer;

        protected override void OnEnter()
        {
            StopMove();
            SetAttack(false);
            SetGuard(false);
            _timer = 0f;
        }

        protected override void OnUpdate()
        {
            if (Control.GetIsDead()) { ForceChangeState<EnemyDeadState>(); return; }
            if (ShouldFlee()) { ChangeState<EnemyFleeState>(); return; }

            _timer += Time.deltaTime;
            if (_timer < AIData.globalCooldown) return;

            if (IsTargetInAttackRange())
                ChangeState<EnemyAttackState>();
            else if (IsTargetInDetectionRange())
                ChangeState<EnemyChaseState>();
            else
                ChangeState<EnemyIdleState>();
        }
    }
}
