using System;
using Monster;

namespace Character
{
    /// <summary>キャラクターのレアリティ（アイテムと同一体系）</summary>
    public enum CharacterRarity
    {
        Common    = 0,
        Uncommon  = 1,
        Rare      = 2,
        Epic      = 3,
        Legendary = 4,
        HyperRare = 5,
    }

    /// <summary>パーティスロット占有サイズ</summary>
    public enum CharacterSize
    {
        /// <summary>0.5枠</summary>
        Small  = 0,
        /// <summary>1.0枠</summary>
        Normal = 1,
    }

    /// <summary>AI行動傾向（パートナー操作時に参照）</summary>
    public enum CharacterBehavior
    {
        Aggressive,
        Defensive,
        Support,
        Passive,
    }

    /// <summary>
    /// キャラクターの基礎/現在ステータス。
    /// baseStatus はレベル・錬金術で変動する素値。
    /// currentStatus は装備・Buff適用後の実効値。
    /// </summary>
    [Serializable]
    public struct CharacterStatus
    {
        public int   maxHp;
        public int   maxStamina;
        public float attackPower;
        public float defensePower;
        public float moveSpeed;
        public float baseAttributePower;
        public float baseResistancePower;
    }

    /// <summary>
    /// プレイヤーが所持するキャラクター1体分のランタイムデータ。
    /// UI なし・データ構造とアクセサのみ。
    /// ES3 / PlayFab でシリアライズされる想定のため [Serializable] を付与。
    /// </summary>
    [Serializable]
    public class OwnedCharacterData
    {
        // ── 識別 ──────────────────────────────────────────────────
        /// <summary>キャラクター種別ID（CharacterControl._characterId と一致）</summary>
        public int characterId;

        /// <summary>所持インスタンスの一意ID（UniqueIDManager 発行）</summary>
        public string uniqueId;

        // ── レベル ────────────────────────────────────────────────
        /// <summary>現在のレベル</summary>
        public int currentLevel;

        /// <summary>初回入手時のレベル（錬金術上限計算に使用）</summary>
        public int originalLevel;

        // ── レアリティ・フラグ ────────────────────────────────────
        public CharacterRarity rarity;

        /// <summary>錬金術の素材・ベースとして使用可能か</summary>
        public bool isAlchemizable;

        // ── ステータス ────────────────────────────────────────────
        /// <summary>素値（レベルアップ・錬金術で変動）</summary>
        public CharacterStatus baseStatus;

        /// <summary>実効値（装備・Buff適用後）</summary>
        public CharacterStatus currentStatus;

        // ── 装備 ──────────────────────────────────────────────────
        /// <summary>装備スロット（10スロット、CharacterInventory と共有）</summary>
        public CharacterSlotItemData equipment;

        // ── パーティ設定 ──────────────────────────────────────────
        public CharacterSize     size;
        public CharacterBehavior behavior;

        // ── ステージ状態 ──────────────────────────────────────────
        /// <summary>出撃メンバーとして選択中か</summary>
        public bool isActive;

        /// <summary>ステージ中に死亡したか（帰還時にリセット）</summary>
        public bool isDeadInStage;

        // ── ヘルパー ──────────────────────────────────────────────
        /// <summary>パーティスロット占有量（Small=0.5, Normal=1.0）</summary>
        public float SlotSize => size == CharacterSize.Small ? 0.5f : 1.0f;

        // ── コンストラクタ ────────────────────────────────────────
        public OwnedCharacterData(
            int characterId,
            string uniqueId,
            int level,
            CharacterRarity rarity,
            CharacterSize size = CharacterSize.Normal,
            CharacterBehavior behavior = CharacterBehavior.Aggressive)
        {
            this.characterId   = characterId;
            this.uniqueId      = uniqueId;
            this.currentLevel  = level;
            this.originalLevel = level;
            this.rarity        = rarity;
            this.size          = size;
            this.behavior      = behavior;
            isAlchemizable     = true;
            isActive           = false;
            isDeadInStage      = false;
            baseStatus         = default;
            currentStatus      = default;
            equipment          = null;
        }
    }
}
