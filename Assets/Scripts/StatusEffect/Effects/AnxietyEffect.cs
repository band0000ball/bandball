using Character.Interfaces;
using Commons;

namespace StatusEffect.Effects
{
    /// <summary>
    /// 不安状態異常
    /// スタミナの自然回復が一定時間停止する
    /// </summary>
    public class AnxietyEffect : StatusEffectBase
    {
        #region Properties

        public override string Name => "Anxiety";
        public override StatusEffectType Type => StatusEffectType.Debuff;

        /// <summary>スタミナ回復が停止しているか</summary>
        public bool IsStaminaRegenBlocked => !IsExpired;

        #endregion

        #region Constructor

        /// <summary>
        /// 不安状態を作成する
        /// </summary>
        /// <param name="duration">持続時間（秒）。省略時はGameBalanceの値を使用</param>
        public AnxietyEffect(float duration = 0)
            : base(duration > 0 ? duration : GameBalance.ANXIETY_STAMINA_BLOCK_DURATION)
        {
        }

        #endregion

        #region Override Methods

        protected override void OnApply()
        {
            // スタミナ回復停止の処理は、CharacterStatsのRegenerateStamina内で
            // この状態をチェックして適用する
        }

        protected override void OnRemove()
        {
            // 解除時の処理（スタミナ回復は自動的に再開）
        }

        #endregion
    }
}