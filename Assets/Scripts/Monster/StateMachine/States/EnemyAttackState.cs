using UnityEngine;

namespace Monster.StateMachine.States
{
    /// <summary>
    /// 攻撃状態。ActionSelector で最適なパターンを選択して実行する。
    /// 攻撃後は CooldownState へ移行する。
    /// </summary>
    public class EnemyAttackState : EnemyStateBase
    {
        private const float AttackDuration = 0.6f;
        private float _attackTimer;
        private AttackPattern _selectedPattern;

        protected override void OnEnter()
        {
            StopMove();
            SetAttack(false);
            _attackTimer = 0f;

            // ActionSelector でパターン選択
            _selectedPattern = ActionSelector.SelectBest(
                Machine.GetCurrentPatterns(),
                DistanceToTarget(),
                Machine.HpRatio,
                Machine.IsPatternReady);

            if (_selectedPattern == null)
            {
                // 実行可能なパターンがなければ Cooldown へ
                ChangeState<EnemyCooldownState>();
                return;
            }

            ExecutePattern(_selectedPattern);
            Machine.StartPatternCooldown(_selectedPattern.patternName, _selectedPattern.cooldown);
        }

        protected override void OnUpdate()
        {
            if (Control.GetIsDead()) { ForceChangeState<EnemyDeadState>(); return; }

            _attackTimer += Time.deltaTime;

            // 攻撃入力は1フレームのみ
            if (_attackTimer > Time.deltaTime * 2)
                SetAttack(false);

            if (_attackTimer >= AttackDuration)
                ChangeState<EnemyCooldownState>();
        }

        protected override void OnExit()
        {
            SetAttack(false);
        }

        private void ExecutePattern(AttackPattern pattern)
        {
            if (pattern.skillSlot < 0)
            {
                // 通常攻撃
                SetAttack(true);
            }
            else
            {
                // スキルスロット起動
                Control.SwapSkillActive(pattern.skillSlot);
                SetAttack(true);
            }
        }
    }
}
