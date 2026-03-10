using UnityEngine;

namespace Monster.StateMachine.States
{
    /// <summary>
    /// 徘徊状態。スポーン地点を中心に patrolRange 内でランダム移動する。
    /// ターゲットを発見したら ChaseState へ移行する。
    /// </summary>
    public class EnemyPatrolState : EnemyStateBase
    {
        private const float DirectionChangeCooldown = 2.5f;
        private float _directionTimer;
        private float _currentDirection;
        private Vector3 _originPosition;

        protected override void OnEnter()
        {
            _originPosition = Control.GetPosition();
            _directionTimer = DirectionChangeCooldown; // 即座に方向決定
            _currentDirection = 0f;
            SetAttack(false);
            SetGuard(false);
        }

        protected override void OnUpdate()
        {
            if (Control.GetIsDead()) { ForceChangeState<EnemyDeadState>(); return; }
            if (ShouldFlee()) { ChangeState<EnemyFleeState>(); return; }
            if (IsTargetInDetectionRange()) { ChangeState<EnemyChaseState>(); return; }

            _directionTimer += Time.deltaTime;
            if (_directionTimer >= DirectionChangeCooldown)
            {
                _directionTimer = 0f;
                _currentDirection = ChoosePatrolDirection();
            }

            SetMove(_currentDirection * AIData.moveSpeed * 0.5f);
        }

        protected override void OnExit()
        {
            StopMove();
        }

        private float ChoosePatrolDirection()
        {
            float posX = Control.GetPosition().x;
            float diffFromOrigin = posX - _originPosition.x;

            // 端に達したら反転
            if (diffFromOrigin > AIData.patrolRange) return -1f;
            if (diffFromOrigin < -AIData.patrolRange) return 1f;

            // ランダムに方向転換（50% で反転）
            return Random.value > 0.5f ? 1f : -1f;
        }
    }
}
