using Commons;
using UnityEngine;

namespace Character.StateMachine.States
{
    /// <summary>
    /// ローリング回避状態。
    /// ROLLING_INVINCIBLE_TIME 秒間は無敵。ROLLING_DURATION 秒後に終了。
    /// CanMove=false にして ProcessMovement の減速をキャンセルし、
    /// OnFixedUpdate でローリング速度を毎フレーム維持する。
    /// TODO: Roll アニメーション・トリガーを Animator に追加する（#27残作業）
    /// </summary>
    public class RollingState : CharacterStateBase
    {
        // Animator に Roll トリガーを追加したら有効化する
        // private static readonly int RollHash = Animator.StringToHash("Roll");

        private float _timer;
        private float _direction;

        public override bool CanMove   => false;
        public override bool CanAttack => false;
        public override bool CanGuard  => false;

        /// <summary>ローリング開始から ROLLING_INVINCIBLE_TIME 秒間が無敵</summary>
        public override bool IsInvincible =>
            _timer > GameBalance.ROLLING_DURATION - GameBalance.ROLLING_INVINCIBLE_TIME;

        protected override void OnEnter()
        {
            _timer     = GameBalance.ROLLING_DURATION;
            _direction = Control.GetRollingDirection();
            Control.StartRollingCooldown();
            // Animator.SetTrigger(RollHash);
        }

        protected override void OnUpdate()
        {
            if (Control.GetIsDead())
            {
                ForceChangeState<DeadState>();
                return;
            }

            _timer -= Time.deltaTime;
            if (_timer > 0f) return;

            // ローリング終了 → 状態を選択
            if (Movement.IsJumping)
                ChangeState<JumpState>();
            else if (Mathf.Abs(Movement.Velocity.x) > GameBalance.MOVEMENT_INPUT_THRESHOLD)
                ChangeState<WalkState>();
            else
                ChangeState<IdleState>();
        }

        protected override void OnFixedUpdate()
        {
            // CanMove=false により ProcessMovement が X 速度を 0 にするため、
            // StateMachine.FixedUpdate（ProcessMovement より後）で上書きして速度を維持する。
            Movement.MaintainRollVelocity(_direction, Control.GetMoveSpeed());
        }
    }
}
