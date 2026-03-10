using Character.Interfaces;
using Commons;

namespace StatusEffect.Effects
{
    /// <summary>
    /// 昏睡（スタン）状態異常
    /// 一定時間、移動・攻撃・ガードが不可能になる
    /// </summary>
    public class StunEffect : StatusEffectBase
    {
        #region Properties

        public override string Name => "Stun";
        public override StatusEffectType Type => StatusEffectType.Disable;

        #endregion

        #region Constructor

        /// <summary>
        /// 昏睡状態を作成する
        /// </summary>
        /// <param name="duration">持続時間（秒）。省略時はGameBalanceの値を使用</param>
        public StunEffect(float duration = 0)
            : base(duration > 0 ? duration : GameBalance.STUN_BASE_DURATION)
        {
        }

        #endregion

        #region Override Methods

        protected override void OnApply()
        {
            if (Target == null) return;

            // 行動を全て封じる
            Target.CanMove = false;
            Target.CanAttack = false;
            Target.CanGuard = false;
        }

        protected override void OnRemove()
        {
            if (Target == null) return;

            // 行動制限を解除
            Target.CanMove = true;
            Target.CanAttack = true;
            Target.CanGuard = true;
        }

        #endregion
    }
}