using System;
using UnityEngine;

namespace Character
{
    /// <summary>
    /// おわかれ時のドロップ候補エントリ（アイテム GUID + 相対重み）
    /// </summary>
    [Serializable]
    public class FarewellItemEntry
    {
        [Tooltip("ドロップするアイテムの GUID（ItemData ScriptableObject の GUID と一致させること）")]
        public string itemGuid;

        [Tooltip("相対ドロップ重み（大きいほど出やすい）")]
        [Min(0f)] public float weight = 1f;
    }

    /// <summary>
    /// キャラクター種別マスターデータ。characterId ごとに 1 アセットを作成する。
    ///
    /// 現在の用途:
    ///   - おわかれ時のアイテムプール定義
    ///
    /// メニュー: Assets > Create > PictureStory > Character > CharacterMasterData
    /// </summary>
    [CreateAssetMenu(menuName = "PictureStory/Character/CharacterMasterData", fileName = "CharacterMasterData_New")]
    public class CharacterMasterData : ScriptableObject
    {
        [Header("識別")]
        [Tooltip("CharacterControl._characterId と一致させること")]
        public int characterId;

        public string characterName;

        [Header("おわかれアイテムプール")]
        [Tooltip("おわかれ時にランダムドロップするアイテム候補。weight の比率で抽選される。")]
        public FarewellItemEntry[] farewellItemPool = new FarewellItemEntry[0];

        [Tooltip("おわかれ時のドロップ個数（0 = ドロップなし）")]
        [Min(0)] public int farewellDropCount = 1;
    }
}
