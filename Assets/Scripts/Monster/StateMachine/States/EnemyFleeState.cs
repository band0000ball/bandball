using UnityEngine;

namespace Monster.StateMachine.States
{
    /// <summary>
    /// 逃走状態。HP が fleeThreshold 以下になったときに発動する。
    /// ターゲットから十分離れたら RecoveryState へ移行する。
    /// HP が reengageThreshold 以上に回復したら ChaseState へ戻る。
    /// </summary>
    public class EnemyFleeState : EnemyStateBase
    {
        private const float SafeDistance = 12f;

        protected override void OnEnter()
        {
            SetAttack(false);
            SetGuard(false);
        }

        protected override void OnUpdate()
        {
            if (Control.GetIsDead()) { ForceChangeState<EnemyDeadState>(); return; }

            // HP 回復で再突撃
            if (Machine.HpRatio >= AIData.reengageThreshold)
            {
                ChangeState<EnemyChaseState>();
                return;
            }

            float dist = DistanceToTarget();

            // 十分離れたら回復状態へ
            if (dist >= SafeDistance)
            {
                ChangeState<EnemyRecoveryState>();
                return;
            }

            // ターゲットと逆方向へ逃げる
            SetMove(-DirectionToTarget() * AIData.moveSpeed * Machine.SpeedMultiplier);
        }

        protected override void OnExit()
        {
            StopMove();
        }
    }
}
