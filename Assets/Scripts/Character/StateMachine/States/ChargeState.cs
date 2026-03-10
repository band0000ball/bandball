using Commons;
using UnityEngine;

namespace Character.StateMachine.States
{
    /// <summary>
    /// チャージ攻撃の溜め状態。
    ///
    /// 遷移元: IdleState / WalkState（GetChargeReady() = true のとき）
    /// 遷移先:
    ///   - ChargeAttackState: 攻撃ボタン離し → SetChargeLevel → 発射
    ///   - IdleState: ガードボタンでキャンセル
    ///   - DamagedState / DeadState: 割り込み
    ///
    /// CanMove: チャージ歩行スキル未解放時は false
    /// チャージ段階は ChargeState 内部タイマーで STAGE1/2/3 を判定する。
    /// </summary>
    public class ChargeState : CharacterStateBase
    {
        private float _stageTimer;
        private int _currentStage;

        public override bool CanMove   => Control.IsChargeWalkUnlocked();
        public override bool CanAttack => false;
        public override bool CanGuard  => false;

        protected override void OnEnter()
        {
            _stageTimer   = 0f;
            _currentStage = 0;
            Animator.SetTrigger("ChargeStart");
        }

        protected override void OnUpdate()
        {
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

            // ガードボタンでキャンセル
            if (Control.GetCrouching())
            {
                ChangeState<IdleState>();
                return;
            }

            // 段階タイマー更新
            _stageTimer += Time.deltaTime;
            int newStage = CalculateStage(_stageTimer);
            if (newStage != _currentStage)
            {
                _currentStage = newStage;
                Animator.SetInteger("ChargeStage", _currentStage);
            }

            // ボタンを離したら発射
            if (!Control.GetAttackInput())
            {
                Control.SetChargeLevel(_currentStage);
                ChangeState<ChargeAttackState>();
            }
        }

        public override bool CanBeInterruptedBy(ICharacterState newState)
            => newState is DamagedState or DeadState;

        private static int CalculateStage(float time)
        {
            if (time >= GameBalance.CHARGE_TIME_STAGE3) return 3;
            if (time >= GameBalance.CHARGE_TIME_STAGE2) return 2;
            if (time >= GameBalance.CHARGE_TIME_STAGE1) return 1;
            return 0;
        }
    }
}
