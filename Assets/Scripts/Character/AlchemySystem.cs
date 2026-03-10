using System.Collections.Generic;

namespace Character
{
    /// <summary>
    /// 錬金術の検証ロジックを提供する静的ユーティリティクラス。
    /// 実行（コレクション操作・通貨処理）は CollectionManager.Alchemize() が担う。
    ///
    /// ── 錬金術条件 ──
    ///   1. baseChar.isAlchemizable == true
    ///   2. baseChar.rarity が HyperRare 未満（これ以上上げられない）
    ///   3. 素材が全て同一 characterId かつ rarity ≥ baseChar.rarity
    ///   4. 素材数 ≥ GetRequiredMaterialCount(baseChar.rarity)
    ///
    /// ── 必要素材数（レアリティ別）──
    ///   Common=1, Uncommon=2, Rare=3, Epic=4, Legendary=5, HyperRare（ベース上限なし）
    ///   ※ Epic 以降は要バランス調整
    ///
    /// depends on: #34a
    /// </summary>
    public static class AlchemySystem
    {
        public enum AlchemyResult
        {
            Success,
            /// <summary>ベースキャラの isAlchemizable が false</summary>
            NotAlchemizable,
            /// <summary>ベースキャラのレアリティが最大（HyperRare）</summary>
            RarityAtMax,
            /// <summary>素材数が不足している</summary>
            InsufficientMaterials,
            /// <summary>素材の characterId またはレアリティ条件が不一致</summary>
            InvalidMaterials,
        }

        /// <summary>
        /// 錬金術の実行前検証を行う。
        /// </summary>
        public static AlchemyResult Validate(
            OwnedCharacterData baseChar,
            IReadOnlyList<OwnedCharacterData> materials)
        {
            if (!baseChar.isAlchemizable)
                return AlchemyResult.NotAlchemizable;

            if (baseChar.rarity >= CharacterRarity.HyperRare)
                return AlchemyResult.RarityAtMax;

            if (materials == null || materials.Count == 0)
                return AlchemyResult.InsufficientMaterials;

            int required = GetRequiredMaterialCount(baseChar.rarity);
            if (materials.Count < required)
                return AlchemyResult.InsufficientMaterials;

            foreach (var m in materials)
            {
                if (m == null)
                    return AlchemyResult.InvalidMaterials;
                if (m.characterId != baseChar.characterId)
                    return AlchemyResult.InvalidMaterials;
                if (m.rarity < baseChar.rarity)
                    return AlchemyResult.InvalidMaterials;
                if (m == baseChar)
                    return AlchemyResult.InvalidMaterials; // 自分自身は素材不可
            }

            return AlchemyResult.Success;
        }

        /// <summary>
        /// 指定レアリティの錬金術に必要な素材数を返す。
        /// </summary>
        public static int GetRequiredMaterialCount(CharacterRarity rarity) => rarity switch
        {
            CharacterRarity.Common    => 1,
            CharacterRarity.Uncommon  => 2,
            CharacterRarity.Rare      => 3,
            CharacterRarity.Epic      => 4,  // 要バランス調整
            CharacterRarity.Legendary => 5,  // 要バランス調整
            CharacterRarity.HyperRare => 6,  // HyperRare はベースになれないが念のため
            _                         => 1,
        };

        /// <summary>
        /// 結果コードを日本語メッセージに変換する（デバッグ・UI 用）。
        /// </summary>
        public static string GetResultMessage(AlchemyResult result) => result switch
        {
            AlchemyResult.Success              => "錬金術が成功しました。",
            AlchemyResult.NotAlchemizable      => "このキャラクターは錬金術の対象外です。",
            AlchemyResult.RarityAtMax          => "レアリティが最大のため錬金術できません。",
            AlchemyResult.InsufficientMaterials => "素材が不足しています。",
            AlchemyResult.InvalidMaterials     => "素材の条件が合いません。",
            _                                  => "不明なエラーです。",
        };
    }
}
