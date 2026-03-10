using Character.StateMachine;

namespace Character.StateMachine.States
{
    /// <summary>
    /// 被ダメージ状態。全アクション不可。Damagedアニメーション終了後にIdleStateへ戻る。
    /// </summary>
    public class DamagedState : CharacterStateBase
    {
        public override bool CanMove => false;
        public override bool CanAttack => false;
        public override bool CanGuard => false;

        private bool _initialized;

        protected override void OnEnter()
        {
            _initialized = false;
        }

        protected override void OnUpdate()
        {
            // アニメーション切り替わりを1フレーム待ってから終了検出を開始する
            if (!_initialized)
            {
                _initialized = true;
                return;
            }

            if (Control.GetIsDead())
            {
                ForceChangeState<DeadState>();
                return;
            }

            if (!Animator.GetCurrentAnimatorStateInfo(0).IsName("Damaged"))
            {
                ChangeState<IdleState>();
            }
        }
    }
}