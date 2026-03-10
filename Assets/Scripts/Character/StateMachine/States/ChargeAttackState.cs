namespace Character.StateMachine.States
{
    /// <summary>
    /// チャージ攻撃の発射状態。
    ///
    /// 遷移元: ChargeState（ボタン離し）
    /// 遷移先:
    ///   - IdleState: ChargeRelease アニメーション終了後
    ///   - DamagedState / DeadState: 割り込み
    ///
    /// OnEnter で ChargeAttackMotion() を呼び ChargeRelease トリガーをセット。
    /// アニメーション終了を検出して IdleState へ戻る。
    /// ダメージ倍率は CharacterCombat._chargeDamageMultiplier に格納済みで、
    /// DamageManager.Damage() から GetChargeDamageMultiplier() で取得される。
    /// </summary>
    public class ChargeAttackState : CharacterStateBase
    {
        private bool _initialized;

        public override bool CanMove   => false;
        public override bool CanAttack => false;
        public override bool CanGuard  => false;

        protected override void OnEnter()
        {
            _initialized = false;
            Control.ChargeAttackMotion(); // CharacterCombat にチャージ倍率を設定し Animator Trigger
        }

        protected override void OnUpdate()
        {
            // アニメーション開始を 1 フレーム待つ
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

            // ChargeRelease アニメーション（段階によって名前が異なる場合も対応）
            var animState = Animator.GetCurrentAnimatorStateInfo(0);
            bool isChargeAnim = animState.IsName("ChargeRelease1") ||
                                animState.IsName("ChargeRelease2") ||
                                animState.IsName("ChargeRelease3");
            if (!isChargeAnim)
                ChangeState<IdleState>();
        }

        public override bool CanBeInterruptedBy(ICharacterState newState)
            => newState is DamagedState or DeadState;
    }
}
