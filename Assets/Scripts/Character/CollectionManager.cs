using System.Collections.Generic;
using System.Linq;
using Databrain;
using Items.ItemData;
using Level;
using Manager;
using UnityEngine;

namespace Character
{
    /// <summary>
    /// コレクション管理マネージャー。「おわかれ」「スクラップ」「錬金術」と pending キャラの解決を担う。
    ///
    /// ── おわかれ ──
    ///   CharacterMasterData.farewellItemPool から weight 抽選でアイテム GUID を返す。
    ///   実際のインベントリ追加は呼び出し元（UI 側）で行う。
    ///
    /// ── スクラップ ──
    ///   返還通貨 = (originalLevel から currentLevel までのレベルアップ総費用) × 0.5
    ///   PlayerWallet へ自動加算する。
    ///
    /// ── 錬金術 ──
    ///   AlchemySystem.Validate() で条件確認後、素材を処理（装備返却・通貨還元・削除）し
    ///   ベースのレアリティを +1 する。
    ///
    /// ── pending 解決 ──
    ///   コレクション満員時に一時保持された PendingCharacter を、
    ///   (A) 既存キャラを削除して pending を登録、または (B) pending を破棄する。
    ///
    /// depends on: #34a, #34d, #34e
    /// </summary>
    public class CollectionManager : MonoBehaviour
    {
        public static CollectionManager Instance { get; private set; }

        [Header("データ参照")]
        [SerializeField] private DataLibrary _dataLibrary;

        [Tooltip("characterId をキーにしてマスターデータを参照するテーブル")]
        [SerializeField] private CharacterMasterData[] _masterDataTable = new CharacterMasterData[0];

        // ── イベント ──────────────────────────────────────────────────────────

        /// <summary>おわかれ完了時に発火。引数はドロップされたアイテム GUID 一覧（UI 表示用）。</summary>
        public static event System.Action<OwnedCharacterData, IReadOnlyList<string>> OnFarewellCompleted;

        /// <summary>スクラップ完了時に発火。引数は返還ゴールド量（UI 表示用）。</summary>
        public static event System.Action<OwnedCharacterData, int> OnScrapCompleted;

        /// <summary>pending キャラの解決完了時に発火。</summary>
        public static event System.Action OnPendingResolved;

        /// <summary>
        /// 錬金術完了時に発火。引数: (ベースキャラ)
        /// </summary>
        public static event System.Action<OwnedCharacterData> OnAlchemyCompleted;

        /// <summary>
        /// 素材キャラの装備返却時に発火。引数: (素材キャラ, 返却アイテムリスト)
        /// 実際のインベントリ追加は UI 側で行う（#34c / #35 実装待ち）。
        /// </summary>
        public static event System.Action<OwnedCharacterData, OwnedItemData[]> OnEquipmentReturned;

        // ── Unity ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// 指定キャラクターと「おわかれ」する。
        /// コレクションから削除し、farewell アイテムを抽選して返す。
        /// </summary>
        /// <returns>ドロップされたアイテム GUID のリスト（空の場合あり）</returns>
        public List<string> Farewell(OwnedCharacterData data)
        {
            if (data == null) return new List<string>();

            var dropped = RollFarewellItems(data.characterId);

            OwnedCharacterCollection.Instance?.Remove(data);

            OnFarewellCompleted?.Invoke(data, dropped);
            return dropped;
        }

        /// <summary>
        /// 指定キャラクターをスクラップする。
        /// コレクションから削除し、(レベルアップ総費用 × 0.5) を PlayerWallet へ加算する。
        /// </summary>
        /// <returns>返還されたゴールド量</returns>
        public int Scrap(OwnedCharacterData data)
        {
            if (data == null) return 0;

            int refund = CalcScrapRefund(data);

            OwnedCharacterCollection.Instance?.Remove(data);

            if (refund > 0 && PlayerWallet.Instance != null)
                PlayerWallet.Instance.Add(refund);

            OnScrapCompleted?.Invoke(data, refund);
            return refund;
        }

        /// <summary>
        /// pending キャラをおわかれして、pending を解除する（pending を破棄）。
        /// </summary>
        public List<string> FarewellPending()
        {
            var col = OwnedCharacterCollection.Instance;
            if (col == null || !col.HasPending) return new List<string>();

            var pending = col.PendingCharacter;
            var dropped = RollFarewellItems(pending.characterId);
            col.ClearPending();

            OnFarewellCompleted?.Invoke(pending, dropped);
            OnPendingResolved?.Invoke();
            return dropped;
        }

        /// <summary>
        /// pending キャラをスクラップして、pending を解除する（pending を破棄）。
        /// </summary>
        public int ScrapPending()
        {
            var col = OwnedCharacterCollection.Instance;
            if (col == null || !col.HasPending) return 0;

            var pending = col.PendingCharacter;
            int refund = CalcScrapRefund(pending);
            col.ClearPending();

            if (refund > 0 && PlayerWallet.Instance != null)
                PlayerWallet.Instance.Add(refund);

            OnScrapCompleted?.Invoke(pending, refund);
            OnPendingResolved?.Invoke();
            return refund;
        }

        /// <summary>
        /// 既存キャラを削除して pending キャラをコレクションに登録する。
        /// </summary>
        /// <param name="toRemove">削除する既存キャラ（おわかれまたはスクラップ済みであること）</param>
        public void RegisterPendingAfterRemoval(OwnedCharacterData toRemove, bool useScrap)
        {
            var col = OwnedCharacterCollection.Instance;
            if (col == null || !col.HasPending) return;

            var pending = col.PendingCharacter;

            // 既存キャラを削除
            if (useScrap) Scrap(toRemove);
            else Farewell(toRemove);

            // pending をコレクションへ登録
            col.ClearPending();
            col.TryAdd(pending);

            OnPendingResolved?.Invoke();
        }

        // ── Private ───────────────────────────────────────────────────────────

        /// <summary>
        /// CharacterMasterData の farewellItemPool から weight 抽選を行い、
        /// ドロップされたアイテム GUID のリストを返す。
        /// </summary>
        private List<string> RollFarewellItems(int characterId)
        {
            var result = new List<string>();
            var master = FindMasterData(characterId);
            if (master == null || master.farewellItemPool.Length == 0 || master.farewellDropCount <= 0)
                return result;

            // 総重みを計算
            float totalWeight = 0f;
            foreach (var e in master.farewellItemPool)
                totalWeight += Mathf.Max(0f, e.weight);

            if (totalWeight <= 0f) return result;

            for (int i = 0; i < master.farewellDropCount; i++)
            {
                float roll = Random.Range(0f, totalWeight);
                float cumulative = 0f;
                foreach (var e in master.farewellItemPool)
                {
                    cumulative += Mathf.Max(0f, e.weight);
                    if (roll <= cumulative)
                    {
                        result.Add(e.itemGuid);
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// スクラップ返還ゴールドを計算する。
        /// 返還額 = (originalLevel → currentLevel のレベルアップ総費用) × 0.5 (切り捨て)
        /// </summary>
        private int CalcScrapRefund(OwnedCharacterData data)
        {
            if (_dataLibrary == null || data.currentLevel <= data.originalLevel) return 0;

            int total = LevelUpManager.GetTotalLevelUpCost(
                _dataLibrary, data.originalLevel, data.currentLevel);

            return Mathf.FloorToInt(total * 0.5f);
        }

        private CharacterMasterData FindMasterData(int characterId)
        {
            foreach (var m in _masterDataTable)
            {
                if (m != null && m.characterId == characterId) return m;
            }
            return null;
        }

        // ── Alchemy ───────────────────────────────────────────────────────────

        /// <summary>
        /// 錬金術を実行する。
        /// 成功時: 素材を処理（装備返却 + 通貨還元 + コレクション削除）し、
        ///         ベースのレアリティを +1 する。
        /// </summary>
        /// <param name="baseChar">強化対象キャラクター</param>
        /// <param name="materials">素材キャラクターリスト</param>
        /// <returns>AlchemySystem.AlchemyResult</returns>
        public AlchemySystem.AlchemyResult Alchemize(
            OwnedCharacterData baseChar,
            List<OwnedCharacterData> materials)
        {
            var result = AlchemySystem.Validate(baseChar, materials);
            if (result != AlchemySystem.AlchemyResult.Success) return result;

            var col = OwnedCharacterCollection.Instance;

            foreach (var m in materials)
            {
                // 装備を返却（イベント発火で UI 側に委ねる）
                ReturnMaterialEquipment(m);

                // レベルアップ費用 × 0.5 を通貨還元
                int refund = CalcScrapRefund(m);
                if (refund > 0) PlayerWallet.Instance?.Add(refund);

                // コレクションから削除
                col?.Remove(m);
            }

            // ベースのレアリティを1段階上昇
            baseChar.rarity = (CharacterRarity)((int)baseChar.rarity + 1);

            OnAlchemyCompleted?.Invoke(baseChar);
            return AlchemySystem.AlchemyResult.Success;
        }

        private void ReturnMaterialEquipment(OwnedCharacterData material)
        {
            if (material.equipment == null) return;

            var items = material.equipment.Items()
                .Where(x => x != null)
                .ToArray();

            if (items.Length == 0) return;

            OnEquipmentReturned?.Invoke(material, items);
        }
    }
}
