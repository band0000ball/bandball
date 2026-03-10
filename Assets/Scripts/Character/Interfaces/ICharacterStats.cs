namespace Character.Interfaces
{
    /// <summary>
    /// キャラクターのステータスを管理するインターフェース
    /// CharacterControlから分離してテスト可能性と拡張性を向上させる
    /// </summary>
    public interface ICharacterStats
    {
        #region Health

        /// <summary>現在のHP</summary>
        float Health { get; }

        /// <summary>最大HP</summary>
        float MaxHealth { get; }

        /// <summary>死亡しているか</summary>
        bool IsDead { get; }

        #endregion

        #region Stamina

        /// <summary>現在のスタミナ</summary>
        float Stamina { get; }

        /// <summary>最大スタミナ</summary>
        float MaxStamina { get; }

        #endregion

        #region Combat Stats

        /// <summary>攻撃力（バフ込み）</summary>
        float AttackPower { get; }

        /// <summary>防御力（バフ込み）</summary>
        float DefensePower { get; }

        /// <summary>シールド値</summary>
        float Shield { get; }

        /// <summary>クリティカルダメージ倍率</summary>
        float CriticalDamage { get; }

        /// <summary>クリティカル発生率</summary>
        float CriticalChance { get; }

        /// <summary>シールド特攻倍率</summary>
        float ShieldAttackRate { get; }

        /// <summary>運（ダメージ変動・ドロップ率に影響）</summary>
        float Luck { get; }

        #endregion

        #region State Flags (for Status Effects)

        /// <summary>移動可能か（昏睡等で制限）</summary>
        bool CanMove { get; set; }

        /// <summary>攻撃可能か</summary>
        bool CanAttack { get; set; }

        /// <summary>ガード可能か</summary>
        bool CanGuard { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        /// <param name="damage">ダメージ量</param>
        void TakeDamage(float damage);

        /// <summary>
        /// 回復する
        /// </summary>
        /// <param name="amount">回復量</param>
        /// <param name="allowOverheal">オーバーヒールを許可するか</param>
        void Heal(float amount, bool allowOverheal = false);

        /// <summary>
        /// スタミナを消費する
        /// </summary>
        /// <param name="amount">消費量</param>
        /// <returns>消費に成功したか</returns>
        bool ConsumeStamina(float amount);

        /// <summary>
        /// スタミナを回復する（自然回復用）
        /// </summary>
        /// <param name="deltaTime">経過時間</param>
        void RegenerateStamina(float deltaTime);

        /// <summary>
        /// シールドを減少させる
        /// </summary>
        /// <param name="amount">減少量</param>
        void DecreaseShield(float amount);

        #endregion
    }
}
