using UnityEngine;

namespace Monster.StateMachine.States
{
    /// <summary>
    /// 待機状態。ターゲットを探し、発見したら ChaseState へ移行する。
    /// 一定時間後に PatrolState へ移行してランダム徘徊を行う。
    /// </summary>
    public class EnemyIdleState : EnemyStateBase
    {
        private const float PatrolTransitionTime = 3f;
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
            if (IsTargetInDetectionRange()) { ChangeState<EnemyChaseState>(); return; }

            _timer += Time.deltaTime;
            if (_timer >= PatrolTransitionTime)
                ChangeState<EnemyPatrolState>();
        }
    }
}
