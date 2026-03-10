using Character.StateMachine;
using Commons;
using UnityEngine;

namespace Character.StateMachine.States
{
    /// <summary>
    /// ガード状態。移動・攻撃不可。
    /// OnEnterでパリィ窓を開く。窓内に攻撃を受けるとパリィ成功し、
    /// PARRY_COUNTER_WINDOW 秒間の反撃チャンスが生まれる。
    /// TODO: ParrySuccess アニメーション・トリガーを Animator に追加する
    /// </summary>
    public class GuardState : CharacterStateBase
    {
        // private static readonly int ParrySuccessHash = Animator.StringToHash("ParrySuccess");

        private bool _parrySuccess;
        private float _parryCounterTimer;

        public override bool CanMove   => false;
        public override bool CanAttack => false;

        protected override void OnEnter()
        {
            _parrySuccess = false;
            _parryCounterTimer = 0f;
            Combat.StartParryWindow();
        }

        protected override void OnUpdate()
        {
            // パリィタイマーを毎フレーム減算
            Combat.DecreaseParryTimer(Time.deltaTime);

            if (Control.GetIsDead())
            {
                ForceChangeState<DeadState>();
                return;
            }

            // パリィ成功チェック
            if (Combat.ConsumeParry())
            {
                Control.RecoverStaminaOnParry();
                _parrySuccess = true;
                _parryCounterTimer = GameBalance.PARRY_COUNTER_WINDOW;
                // Animator.SetTrigger(ParrySuccessHash);
            }

            // 反撃チャンス: 攻撃ボタンでAttackStateへ遷移
            if (_parrySuccess)
            {
                _parryCounterTimer -= Time.deltaTime;
                if (Control.GetAttackInput())
                {
                    Combat.AttackMotion(isOnUI: false, attackInput: true);
                    ChangeState<AttackState>();
                    return;
                }
                if (_parryCounterTimer <= 0f)
                    _parrySuccess = false;
            }

            if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Damaged"))
            {
                ChangeState<DamagedState>();
                return;
            }

            // ガード解除（反撃チャンス中はガードを離しても維持）
            if (!Control.GetCrouching() && !_parrySuccess)
            {
                ChangeState<IdleState>();
            }
        }
    }
}