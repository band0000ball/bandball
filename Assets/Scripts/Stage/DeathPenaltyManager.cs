using System.Collections.Generic;
using Character;
using Commons;
using Items.ItemData;
using Manager;
using UnityEngine;

namespace Stage
{
    /// <summary>
    /// 死亡ペナルティを適用するシングルトン。
    /// PlayerCharacterController.OnGameOver と各キャラの OnCharacterDied を購読し、
    /// 現在の DifficultySettings に従って処理する。
    ///
    /// ペナルティ内容（難易度別）:
    ///   Normal : 所持金 10% ロスト
    ///   Hard   : 所持金 50% ロスト + 装備の一部ドロップ（10分タイマー）
    ///   Ultra  : 所持金 100% ロスト + 装備全ドロップ + キャラドロップ（30分タイマー）
    ///
    /// depends on: #16 DifficultyManager, #34a OwnedCharacterData, #34b PlayerCharacterController
    /// </summary>
    public class DeathPenaltyManager : MonoBehaviour
    {
        public static DeathPenaltyManager Instance { get; private set; }

        /// <summary>ドロップ回収物が追加されたとき（UI 更新用）。</summary>
        public static event System.Action<DroppedPickup> OnPickupSpawned;

        /// <summary>回収物が回収されたとき。</summary>
        public static event System.Action<DroppedPickup> OnPickupRecovered;

        /// <summary>回収物がタイムアウトして消滅したとき。</summary>
        public static event System.Action<DroppedPickup> OnPickupLost;

        [Header("ドロッププレハブ")]
        [SerializeField] private GameObject _droppedPickupPrefab;

        private readonly List<OwnedCharacterData> _trackedCharacters = new();
        private readonly Dictionary<OwnedCharacterData, (CharacterControl cc, System.Action listener)> _deathListeners = new();

        // ── Unity ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            var pcc = PlayerCharacterController.Instance;
            if (pcc != null)
                pcc.OnGameOver += OnGameOver;
        }

        private void OnDisable()
        {
            var pcc = PlayerCharacterController.Instance;
            if (pcc != null)
                pcc.OnGameOver -= OnGameOver;

            UntrackAll();
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// ステージ開始時に出撃キャラを登録する。
        /// DeathPenaltyManager は各キャラの OnCharacterDied を購読する。
        /// </summary>
        public void TrackCharacters(IEnumerable<CharacterControl> characters)
        {
            UntrackAll();
            foreach (var cc in characters)
            {
                if (cc == null) continue;
                var data = GetOwnedData(cc);
                if (data == null) continue;

                _trackedCharacters.Add(data);

                void listener() => OnCharacterDied(cc, data);
                _deathListeners[data] = (cc, listener);
                cc.OnCharacterDied += listener;
            }
        }

        /// <summary>DroppedPickup から呼ばれる: プレイヤーが回収した。</summary>
        public void OnPickupCollected(DroppedPickup pickup)
        {
            ApplyPickupRecovery(pickup);
            OnPickupRecovered?.Invoke(pickup);
        }

        /// <summary>DroppedPickup から呼ばれる: タイムアウトで消滅した。</summary>
        public void OnPickupExpired(DroppedPickup pickup)
        {
            if (pickup.Type == DroppedPickup.PickupType.Character &&
                pickup.CharacterData != null)
            {
                // キャラクター永久ロスト
                OwnedCharacterCollection.Instance?.Remove(pickup.CharacterData);
                Debug.Log($"[DeathPenaltyManager] キャラクター永久ロスト: ID {pickup.CharacterData.characterId}");
            }
            OnPickupLost?.Invoke(pickup);
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void OnGameOver()
        {
            // ゲームオーバー時はノーマル難易度でも金銭ペナルティを適用
            ApplyGoldPenalty();
            DifficultyManager.Instance?.UnlockAfterStage();
        }

        private void OnCharacterDied(CharacterControl cc, OwnedCharacterData data)
        {
            var settings = DifficultyManager.Instance?.Current;
            if (settings == null) return;

            var deathPos = cc.transform.position;

            ApplyGoldPenalty();

            if (settings.dropEquipmentOnDeath && data.equipment != null)
                SpawnEquipmentDrops(data, deathPos, settings);

            if (settings.dropCharacterOnDeath)
                SpawnCharacterDrop(data, deathPos, settings.characterRecoveryTimeLimit);
        }

        private void ApplyGoldPenalty()
        {
            var wallet = PlayerWallet.Instance;
            if (wallet == null) return;

            var settings = DifficultyManager.Instance?.Current;
            if (settings == null) return;

            int loss = Mathf.FloorToInt(wallet.Gold * settings.currencyLossRate);
            if (loss <= 0) return;

            if (settings.difficulty == StageDifficulty.Ultra)
            {
                // ウルトラ: ドロップとして回収可能
                wallet.TrySpend(loss);
                SpawnGoldDrop(loss, GetPenaltyPosition(), settings.itemRecoveryTimeLimit);
            }
            else
            {
                wallet.TrySpend(loss);
            }
        }

        private void SpawnEquipmentDrops(OwnedCharacterData data, Vector3 pos, DifficultySettings settings)
        {
            if (_droppedPickupPrefab == null || data.equipment == null) return;

            var items = data.equipment.Items();
            if (items == null) return;

            foreach (var item in items)
            {
                if (item == null) continue;

                // ハードは確率でドロップ、ウルトラは全ドロップ
                if (!settings.dropAllEquipmentOnDeath)
                {
                    float dropRate = GameBalance.HARD_EQUIP_DROP_BASE_RATE *
                                     GetRarityDropModifier(item);
                    if (UnityEngine.Random.value > dropRate) continue;
                }

                var offset = UnityEngine.Random.insideUnitCircle * 0.5f;
                var dropPos = pos + new Vector3(offset.x, offset.y, 0f);
                SpawnPickup(pos: dropPos).InitializeEquipment(item, settings.itemRecoveryTimeLimit);
            }
        }

        private void SpawnCharacterDrop(OwnedCharacterData data, Vector3 pos, float timeLimit)
        {
            if (_droppedPickupPrefab == null) return;
            data.isDeadInStage = true;
            SpawnPickup(pos).InitializeCharacter(data, timeLimit);
        }

        private void SpawnGoldDrop(int amount, Vector3 pos, float timeLimit)
        {
            if (_droppedPickupPrefab == null) return;
            SpawnPickup(pos).InitializeGold(amount, timeLimit);
        }

        private DroppedPickup SpawnPickup(Vector3 pos)
        {
            var go      = Instantiate(_droppedPickupPrefab, pos, Quaternion.identity);
            var pickup  = go.GetComponent<DroppedPickup>();
            OnPickupSpawned?.Invoke(pickup);
            return pickup;
        }

        private void ApplyPickupRecovery(DroppedPickup pickup)
        {
            switch (pickup.Type)
            {
                case DroppedPickup.PickupType.Gold:
                    PlayerWallet.Instance?.Add(pickup.GoldAmount);
                    break;
                case DroppedPickup.PickupType.Equipment:
                    // TODO: インベントリシステム実装後にアイテムを戻す
                    break;
                case DroppedPickup.PickupType.Character:
                    if (pickup.CharacterData != null)
                        pickup.CharacterData.isDeadInStage = false;
                    break;
            }
        }

        private void UntrackAll()
        {
            foreach (var (_, entry) in _deathListeners)
            {
                if (entry.cc != null)
                    entry.cc.OnCharacterDied -= entry.listener;
            }
            _deathListeners.Clear();
            _trackedCharacters.Clear();
        }

        private static OwnedCharacterData GetOwnedData(CharacterControl cc)
        {
            var col = OwnedCharacterCollection.Instance;
            if (col == null) return null;
            foreach (var d in col.Characters)
                if (d.characterId == cc.CharacterId) return d;
            return null;
        }

        private static Vector3 GetPenaltyPosition()
        {
            var pcc = PlayerCharacterController.Instance;
            return pcc?.CurrentOperator?.transform.position ?? Vector3.zero;
        }

        private static float GetRarityDropModifier(OwnedItemData item)
        {
            // アイテムレアリティが高いほどドロップしやすい（コモンは低確率）
            // TODO: OwnedItemData にレアリティ情報が追加されたら参照する
            return 1.0f;
        }
    }
}
