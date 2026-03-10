using System;
using Character;
using Commons;
using Items.ItemData;
using UnityEngine;

namespace Stage
{
    /// <summary>
    /// 死亡ペナルティで世界にドロップした回収物（アイテム/装備/キャラクター）。
    /// 制限時間内に触れると DeathPenaltyManager.OnPickupCollected を呼ぶ。
    ///
    /// セットアップ:
    ///   - Collider2D (isTrigger = true) を付与すること
    ///   - DeathPenaltyManager が Instantiate し Initialize() を呼ぶ
    /// </summary>
    public class DroppedPickup : MonoBehaviour
    {
        public enum PickupType { Gold, Equipment, Character }

        /// <summary>回収物の種別。</summary>
        public PickupType Type { get; private set; }

        /// <summary>Gold の場合の金額。</summary>
        public int GoldAmount { get; private set; }

        /// <summary>Equipment の場合のアイテムデータ。</summary>
        public OwnedItemData ItemData { get; private set; }

        /// <summary>Character の場合のキャラクターデータ。</summary>
        public OwnedCharacterData CharacterData { get; private set; }

        /// <summary>タイムアウトで消滅する UnixEpoch 秒（DateTime.UtcNow との比較用）。</summary>
        public DateTime ExpiresAt { get; private set; }

        /// <summary>残り時間（秒）。</summary>
        public float RemainingSeconds => Mathf.Max(0f, (float)(ExpiresAt - DateTime.UtcNow).TotalSeconds);

        /// <summary>タイムアウトしたか。</summary>
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

        private static readonly string PlayerTag = "Player";

        // ── 初期化 ──────────────────────────────────────────────────────────

        public void InitializeGold(int amount, float timeLimitSeconds)
        {
            Type       = PickupType.Gold;
            GoldAmount = amount;
            ExpiresAt  = DateTime.UtcNow.AddSeconds(timeLimitSeconds);
        }

        public void InitializeEquipment(OwnedItemData item, float timeLimitSeconds)
        {
            Type      = PickupType.Equipment;
            ItemData  = item;
            ExpiresAt = DateTime.UtcNow.AddSeconds(timeLimitSeconds);
        }

        public void InitializeCharacter(OwnedCharacterData charData, float timeLimitSeconds)
        {
            Type          = PickupType.Character;
            CharacterData = charData;
            ExpiresAt     = DateTime.UtcNow.AddSeconds(timeLimitSeconds);
        }

        // ── Unity ─────────────────────────────────────────────────────────────

        private void Update()
        {
            if (IsExpired)
            {
                DeathPenaltyManager.Instance?.OnPickupExpired(this);
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag(PlayerTag)) return;
            DeathPenaltyManager.Instance?.OnPickupCollected(this);
            Destroy(gameObject);
        }
    }
}
