using Character.StateMachine;

namespace Character.StateMachine.States
{
    /// <summary>
    /// 攻撃状態。移動・ガード不可。DamagedState/DeadStateによる割り込みのみ許可。
    /// CanAttack=trueはコンボ継続（#24）のために維持する。
    /// </summary>
    public class AttackState : CharacterStateBase
    {
        public override bool CanMove => false;
        public override bool CanGuard => false;

        private bool _initialized;

        protected override void OnEnter()
        {
            _initialized = false;
        }

        protected override void OnUpdate()
        {
            // アニメーション開始を1フレーム待ってから終了検出を開始する
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

            if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Damaged"))
            {
                ForceChangeState<DamagedState>();
                return;
            }

            var animState = Animator.GetCurrentAnimatorStateInfo(0);
            bool isAttacking = animState.IsName("Attack1") || animState.IsName("Attack10_Finisher");
            if (!isAttacking && !Combat.IsComboWindowOpen)
            {
                ChangeState<IdleState>();
            }
        }

        public override bool CanBeInterruptedBy(ICharacterState newState)
        {
            return newState is DamagedState or DeadState;
        }
    }
}