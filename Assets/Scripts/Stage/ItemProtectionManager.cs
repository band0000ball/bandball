using System.Collections.Generic;
using Commons;
using Items.ItemData;
using Manager;
using UnityEngine;

namespace Stage
{
    /// <summary>
    /// ステージ出撃前にアイテムを保護するシングルトン。
    /// 保護されたアイテムは DeathPenaltyManager によるドロップ対象から除外される。
    ///
    /// 保護コスト: GameBalance.ITEM_PROTECTION_COSTS[rarity] ゴールド
    /// 保護状態はステージ終了時（ClearProtection）にリセットされる。
    ///
    /// セットアップ:
    ///   - シーンに配置するか DontDestroyOnLoad オブジェクトに追加する。
    ///   - DeathPenaltyManager.SpawnEquipmentDrops が IsProtected() を呼ぶ（要連携）。
    ///
    /// depends on: #31 DeathPenaltyManager, #16 DifficultyManager
    /// </summary>
    public class ItemProtectionManager : MonoBehaviour
    {
        public static ItemProtectionManager Instance { get; private set; }

        /// <summary>保護状態が変わったとき（UI 更新用）。</summary>
        public static event System.Action<string, bool> OnProtectionChanged;

        // 保護中のアイテムユニークID セット
        private readonly HashSet<string> _protectedIds = new();

        // ── Unity ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>アイテムが保護済みかどうかを返す。</summary>
        public bool IsProtected(string uniqueId) => _protectedIds.Contains(uniqueId);

        /// <summary>HasUniqueIdData のユニークIDで保護済みかどうかを返す。</summary>
        public bool IsProtected(HasUniqueIdData item) =>
            item != null && _protectedIds.Contains(item.UniqueId);

        /// <summary>
        /// アイテムを保護する。成功時 true。
        /// レアリティに応じたゴールドを消費する。
        /// ゴールドが不足している場合や既に保護済みの場合は false。
        /// </summary>
        public bool TryProtect(HasUniqueIdData item)
        {
            if (item == null) return false;
            if (_protectedIds.Contains(item.UniqueId)) return false;

            int cost = GetProtectionCost(item);
            var wallet = PlayerWallet.Instance;
            if (wallet == null || !wallet.TrySpend(cost)) return false;

            _protectedIds.Add(item.UniqueId);
            OnProtectionChanged?.Invoke(item.UniqueId, true);
            Debug.Log($"[ItemProtectionManager] 保護: {item.UniqueId}  コスト {cost}G");
            return true;
        }

        /// <summary>
        /// アイテムの保護を解除する。成功時 true。
        /// ゴールドは返還されない。
        /// </summary>
        public bool RemoveProtection(HasUniqueIdData item)
        {
            if (item == null) return false;
            bool removed = _protectedIds.Remove(item.UniqueId);
            if (removed)
                OnProtectionChanged?.Invoke(item.UniqueId, false);
            return removed;
        }

        /// <summary>ステージ終了時にすべての保護をリセットする。</summary>
        public void ClearProtection()
        {
            foreach (var id in _protectedIds)
                OnProtectionChanged?.Invoke(id, false);
            _protectedIds.Clear();
        }

        /// <summary>アイテムの保護コストを返す（ゴールド）。</summary>
        public static int GetProtectionCost(HasUniqueIdData item)
        {
            if (item == null) return 0;

            int rarity = GetRarityIndex(item);
            var costs  = GameBalance.ITEM_PROTECTION_COSTS;
            rarity = Mathf.Clamp(rarity, 0, costs.Length - 1);
            return costs[rarity];
        }

        // ── Private ───────────────────────────────────────────────────────────

        /// <summary>
        /// アイテムのレアリティを 0 始まりの整数インデックスに変換する。
        /// TODO: HasUniqueIdData にレアリティ情報が追加されたら参照する。
        /// </summary>
        private static int GetRarityIndex(HasUniqueIdData item)
        {
            // 現在はレアリティ情報が未実装のため 0 を返す（最低コスト）
            return 0;
        }
    }
}
