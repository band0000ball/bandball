using Commons;

namespace Stage
{
    /// <summary>ステージ難易度の3段階</summary>
    public enum StageDifficulty
    {
        Normal,
        Hard,
        Ultra,
    }

    /// <summary>
    /// 難易度ごとのパラメーター定義。
    /// DifficultyManager.Current から参照する。
    /// </summary>
    [System.Serializable]
    public class DifficultySettings
    {
        public StageDifficulty difficulty;

        /// <summary>敵レベル補正値（加算）</summary>
        public int enemyLevelBonus;

        /// <summary>通貨ドロップ倍率</summary>
        public float currencyDropMultiplier;

        /// <summary>アイテムドロップ率倍率</summary>
        public float itemDropRateMultiplier;

        /// <summary>レアアイテム率倍率</summary>
        public float rareItemRateMultiplier;

        /// <summary>死亡時通貨ロスト率（0〜1）</summary>
        public float currencyLossRate;

        /// <summary>死亡時に装備の一部をドロップするか（ハード）</summary>
        public bool dropEquipmentOnDeath;

        /// <summary>死亡時に装備を全てドロップするか（ウルトラ）</summary>
        public bool dropAllEquipmentOnDeath;

        /// <summary>死亡時にキャラクターをドロップするか（ウルトラ）</summary>
        public bool dropCharacterOnDeath;

        /// <summary>アイテム/装備の回収制限時間（秒）</summary>
        public float itemRecoveryTimeLimit;

        /// <summary>キャラクターの回収制限時間（秒）</summary>
        public float characterRecoveryTimeLimit;

        // ── ファクトリ ────────────────────────────────────────────────────────

        public static DifficultySettings CreateNormal() => new()
        {
            difficulty                = StageDifficulty.Normal,
            enemyLevelBonus           = 0,
            currencyDropMultiplier    = 1.0f,
            itemDropRateMultiplier    = 1.0f,
            rareItemRateMultiplier    = 1.0f,
            currencyLossRate          = GameBalance.NORMAL_CURRENCY_LOSS_RATE,
            dropEquipmentOnDeath      = false,
            dropAllEquipmentOnDeath   = false,
            dropCharacterOnDeath      = false,
            itemRecoveryTimeLimit     = GameBalance.ITEM_RECOVERY_TIME_LIMIT,
            characterRecoveryTimeLimit = GameBalance.CHARACTER_RECOVERY_TIME_LIMIT,
        };

        public static DifficultySettings CreateHard() => new()
        {
            difficulty                = StageDifficulty.Hard,
            enemyLevelBonus           = 10,
            currencyDropMultiplier    = 1.5f,
            itemDropRateMultiplier    = 1.3f,
            rareItemRateMultiplier    = 1.5f,
            currencyLossRate          = GameBalance.HARD_CURRENCY_LOSS_RATE,
            dropEquipmentOnDeath      = true,
            dropAllEquipmentOnDeath   = false,
            dropCharacterOnDeath      = false,
            itemRecoveryTimeLimit     = GameBalance.ITEM_RECOVERY_TIME_LIMIT,
            characterRecoveryTimeLimit = GameBalance.CHARACTER_RECOVERY_TIME_LIMIT,
        };

        public static DifficultySettings CreateUltra() => new()
        {
            difficulty                = StageDifficulty.Ultra,
            enemyLevelBonus           = 20,
            currencyDropMultiplier    = 2.5f,
            itemDropRateMultiplier    = 2.0f,
            rareItemRateMultiplier    = 3.0f,
            currencyLossRate          = GameBalance.ULTRA_CURRENCY_LOSS_RATE,
            dropEquipmentOnDeath      = true,
            dropAllEquipmentOnDeath   = true,
            dropCharacterOnDeath      = true,
            itemRecoveryTimeLimit     = GameBalance.ITEM_RECOVERY_TIME_LIMIT,
            characterRecoveryTimeLimit = GameBalance.CHARACTER_RECOVERY_TIME_LIMIT,
        };
    }
}
