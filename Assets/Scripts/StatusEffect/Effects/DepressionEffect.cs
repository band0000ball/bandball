using Character.Interfaces;
using Commons;

namespace StatusEffect.Effects
{
    /// <summary>
    /// 憂鬱状態異常
    /// スキルの発動が遅延する（クールダウンが増加）
    /// </summary>
    public class DepressionEffect : StatusEffectBase
    {
        #region Properties

        public override string Name => "Depression";
        public override StatusEffectType Type => StatusEffectType.Debuff;

        /// <summary>スキル遅延倍率</summary>
        public float SkillDelayMultiplier { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// 憂鬱状態を作成する
        /// </summary>
        /// <param name="duration">持続時間（秒）</param>
        /// <param name="delayMultiplier">スキル遅延倍率。省略時はGameBalanceの値を使用</param>
        public DepressionEffect(float duration, float delayMultiplier = 0)
            : base(duration)
        {
            SkillDelayMultiplier = delayMultiplier > 0
                ? delayMultiplier
                : GameBalance.DEPRESSION_SKILL_DELAY_MULTIPLIER;
        }

        #endregion

        #region Override Methods

        protected override void OnApply()
        {
            // スキルクールダウン増加の処理は、スキル発動時にこの状態をチェックして適用
            // CharacterControlまたはSkillManagerで参照する
        }

        protected override void OnRemove()
        {
            // 解除時の処理（特になし）
        }

        #endregion
    }
}