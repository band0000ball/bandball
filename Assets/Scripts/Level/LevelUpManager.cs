using System.Linq;
using Character;
using Databrain;
using Manager;

namespace Level
{
    /// <summary>
    /// キャラクターのレベルアップ処理を行うユーティリティクラス。
    ///
    /// 仕様:
    ///   - 通貨消費制: Databrain の LevelExp テーブルの needForNextLevel がレベルアップ費用（ゴールド）
    ///   - AP 獲得量 = レベルアップ前のレベル値（例: Lv5→6 で 5AP 獲得）
    ///   - ステータスは上昇しない（AP 振り分けで決まる: #13）
    ///   - レベルキャップ: LevelExp テーブルに現在レベルのエントリがなければ上限
    ///     TODO: #34 OwnedCharacterData のレアリティ連携時に LevelRarity.maxLevel と統合
    /// </summary>
    public static class LevelUpManager
    {
        public enum LevelUpResult
        {
            Success,
            AlreadyMaxLevel,
            NotEnoughCurrency,
        }

        /// <summary>
        /// レベルアップを試みる。
        /// 成功した場合は通貨を消費し、キャラクターのレベルと AP を更新する。
        /// </summary>
        public static LevelUpResult TryLevelUp(CharacterControl character)
        {
            int currentLevel = character.GetCurrentLevel();

            // LevelExp テーブルから現在レベルのエントリを検索
            var entry = GetLevelExpEntry(character, currentLevel);
            if (entry == null) return LevelUpResult.AlreadyMaxLevel;

            // 通貨チェック & 消費
            if (PlayerWallet.Instance == null ||
                !PlayerWallet.Instance.TrySpend(entry.needForNextLevel))
                return LevelUpResult.NotEnoughCurrency;

            // AP 獲得量 = レベルアップ前のレベル値
            character.ApplyLevelUp(apGained: currentLevel);

            return LevelUpResult.Success;
        }

        /// <summary>
        /// 次のレベルアップに必要なゴールドを返す。レベル上限の場合は -1。
        /// </summary>
        public static int GetLevelUpCost(CharacterControl character)
        {
            var entry = GetLevelExpEntry(character, character.GetCurrentLevel());
            return entry?.needForNextLevel ?? -1;
        }

        /// <summary>
        /// 現在レベルが上限に達しているか。
        /// </summary>
        public static bool IsAtMaxLevel(CharacterControl character)
        {
            return GetLevelExpEntry(character, character.GetCurrentLevel()) == null;
        }

        /// <summary>
        /// fromLevel から toLevel-1 までのレベルアップ総費用を返す。
        /// CollectionManager のスクラップ計算で使用する。
        /// </summary>
        public static int GetTotalLevelUpCost(DataLibrary library, int fromLevel, int toLevel)
        {
            if (library == null || fromLevel >= toLevel) return 0;

            var table = library.GetAllInitialDataObjectsByType<LevelExp>();
            if (table == null) return 0;

            int total = 0;
            for (int lv = fromLevel; lv < toLevel; lv++)
            {
                var entry = table.FirstOrDefault(x => x.level == lv);
                if (entry != null) total += entry.needForNextLevel;
            }
            return total;
        }

        // ── Private ─────────────────────────────────────────────────────────

        /// <summary>
        /// DataLibrary から指定レベルの LevelExp エントリを取得する。
        /// 見つからない場合は null（= レベル上限）。
        /// NOTE: 呼び出しごとにリスト取得が発生するため、頻繁な呼び出しは避けること。
        /// </summary>
        private static LevelExp GetLevelExpEntry(CharacterControl character, int level)
        {
            var table = character.DataLibrary
                .GetAllInitialDataObjectsByType<LevelExp>();
            return table?.FirstOrDefault(x => x.level == level);
        }
    }
}
