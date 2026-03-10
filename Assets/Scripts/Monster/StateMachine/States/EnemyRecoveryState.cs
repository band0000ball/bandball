using UnityEngine;

namespace Monster.StateMachine.States
{
    /// <summary>
    /// 回復状態。逃走後に待機して HP 回復または時間経過を待つ。
    /// HP が reengageThreshold 以上に戻るか、タイムアウトで ChaseState へ移行する。
    /// </summary>
    public class EnemyRecoveryState : EnemyStateBase
    {
        private const float MaxRecoveryTime = 5f;
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

            _timer += Time.deltaTime;

            bool hpRecovered = Machine.HpRatio >= AIData.reengageThreshold;
            bool timedOut = _timer >= MaxRecoveryTime;

            if (hpRecovered || timedOut)
            {
                if (IsTargetInDetectionRange())
                    ChangeState<EnemyChaseState>();
                else
                    ChangeState<EnemyIdleState>();
            }
        }
    }
}
