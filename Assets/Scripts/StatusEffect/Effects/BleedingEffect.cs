using Character.Interfaces;
using Commons;

namespace StatusEffect.Effects
{
    /// <summary>
    /// 出血状態異常
    /// 一定時間、継続的にダメージを受ける
    /// スタック可能で、スタック数に応じてダメージが増加
    /// </summary>
    public class BleedingEffect : StatusEffectBase
    {
        #region Properties

        public override string Name => "Bleeding";
        public override StatusEffectType Type => StatusEffectType.DamageOverTime;
        public override bool IsStackable => true;

        private readonly float _damagePerSecond;

        #endregion

        #region Constructor

        /// <summary>
        /// 出血状態を作成する
        /// </summary>
        /// <param name="duration">持続時間（秒）</param>
        /// <param name="damagePerSecond">毎秒のダメージ量。省略時はGameBalanceの値を使用</param>
        public BleedingEffect(float duration, float damagePerSecond = 0)
            : base(duration)
        {
            _damagePerSecond = damagePerSecond > 0
                ? damagePerSecond
                : GameBalance.BLEEDING_DAMAGE_PER_SECOND;
        }

        #endregion

        #region Override Methods

        protected override void OnUpdate(float deltaTime)
        {
            if (Target == null) return;

            // スタック数に応じたダメージを与える
            float damage = _damagePerSecond * StackCount * deltaTime;
            Target.TakeDamage(damage);
        }

        protected override void OnStackAdded(int count)
        {
            // スタック追加時に持続時間をリセット（オプション）
            // RemainingTime = InitialDuration;
        }

        #endregion
    }
}