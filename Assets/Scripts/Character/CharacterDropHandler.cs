using UnityEngine;

namespace Character
{
    /// <summary>
    /// ボス撃破時のキャラクタードロップを処理するコンポーネント。
    /// ボス敵の GameObject に CharacterControl と共に AddComponent する。
    ///
    /// ── ドロップ率計算 ──
    ///   最終ドロップ率 = min(baseDropRate × オーバーキル補正, maxDropRate)
    ///   オーバーキル補正:
    ///     比率=0.0（HP ちょうど0） → 0.50倍
    ///     比率=0.5（HP×0.5 超過）  → 0.80倍（ピーク）
    ///     比率=2.0（HP×2.0 超過）  → 0.10倍
    ///
    /// ── 同一キャラ入手 ──
    ///   OnDuplicateCharacterObtained イベント発火 → UI が受け取り選択ダイアログを表示
    ///   コールバック(true)  = A 案: 既存キャラのステータスを +2〜5% アップ
    ///   コールバック(false) = B 案: 追加登録（コレクション枠を消費）
    ///
    /// ── コレクション満員時 ──
    ///   OwnedCharacterCollection.SetPending() に一時保持。
    ///   ゲームオーバー → 消滅。拠点帰還 → 削除操作を強制（#34e で実装）。
    ///
    /// depends on: #34a (OwnedCharacterData / OwnedCharacterCollection)
    /// </summary>
    [RequireComponent(typeof(CharacterControl))]
    public class CharacterDropHandler : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────────

        [Header("ドロップキャラクター設定")]
        [SerializeField] private int _characterId;
        [SerializeField] private int _level = 1;
        [SerializeField] private CharacterRarity _rarity = CharacterRarity.Common;
        [SerializeField] private CharacterSize _size = CharacterSize.Normal;
        [SerializeField] private CharacterBehavior _behavior = CharacterBehavior.Aggressive;

        [Header("ドロップ率")]
        [SerializeField, Range(0f, 1f)] private float _baseDropRate = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _maxDropRate = 0.95f;

        // ── Static Events ─────────────────────────────────────────────────────

        /// <summary>
        /// 新規キャラクター入手時に発火（UI 通知・演出用）。
        /// </summary>
        public static event System.Action<OwnedCharacterData> OnNewCharacterObtained;

        /// <summary>
        /// 同一キャラクター入手時に発火。
        /// 引数: (既存データ, 新規データ, 選択コールバック)
        /// コールバック: true = A案（ステータスアップ）, false = B案（追加登録）
        /// </summary>
        public static event System.Action<OwnedCharacterData, OwnedCharacterData, System.Action<bool>> OnDuplicateCharacterObtained;

        // ── Private ───────────────────────────────────────────────────────────

        private CharacterControl _control;

        // ── Unity ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            _control = GetComponent<CharacterControl>();
        }

        private void OnEnable()
        {
            if (_control != null)
                _control.OnCharacterDied += HandleDeath;
        }

        private void OnDisable()
        {
            if (_control != null)
                _control.OnCharacterDied -= HandleDeath;
        }

        // ── Drop Logic ────────────────────────────────────────────────────────

        private void HandleDeath()
        {
            float overkillRatio = _control.GetOverkillRatio();
            float multiplier = CalcOverkillMultiplier(overkillRatio);
            float finalRate = Mathf.Min(_baseDropRate * multiplier, _maxDropRate);

            if (Random.value > finalRate) return;

            string uniqueId = System.Guid.NewGuid().ToString();
            var data = new OwnedCharacterData(
                _characterId, uniqueId, _level, _rarity, _size, _behavior);

            var collection = OwnedCharacterCollection.Instance;
            if (collection == null)
            {
                Debug.LogWarning("[CharacterDropHandler] OwnedCharacterCollection が見つかりません。");
                return;
            }

            var existing = FindSameCharacter(collection);
            if (existing != null)
            {
                // 重複: UI へ選択を委ねる
                OnDuplicateCharacterObtained?.Invoke(existing, data, choice =>
                {
                    if (choice)
                        ApplyStatBoost(existing);   // A案: ステータスアップ
                    else
                        RegisterToCollection(collection, data); // B案: 追加登録
                });
            }
            else
            {
                RegisterToCollection(collection, data);
            }
        }

        private void RegisterToCollection(OwnedCharacterCollection collection, OwnedCharacterData data)
        {
            if (collection.IsFull)
            {
                collection.SetPending(data);
                Debug.Log($"[CharacterDropHandler] コレクション満員のため pending に保持: characterId={data.characterId}");
            }
            else
            {
                collection.TryAdd(data);
                OnNewCharacterObtained?.Invoke(data);
            }
        }

        private OwnedCharacterData FindSameCharacter(OwnedCharacterCollection collection)
        {
            foreach (var c in collection.Characters)
            {
                if (c.characterId == _characterId) return c;
            }
            return null;
        }

        /// <summary>
        /// 既存キャラのbaseStatusを全項目 +2〜5%（レアリティ依存）アップする。
        /// </summary>
        private void ApplyStatBoost(OwnedCharacterData target)
        {
            float rate = GetBoostRate(target.rarity);
            var s = target.baseStatus;
            s.maxHp = Mathf.RoundToInt(s.maxHp * (1f + rate));
            s.maxStamina = Mathf.RoundToInt(s.maxStamina * (1f + rate));
            s.attackPower *= (1f + rate);
            s.defensePower *= (1f + rate);
            s.moveSpeed *= (1f + rate);
            s.baseAttributePower *= (1f + rate);
            s.baseResistancePower *= (1f + rate);
            target.baseStatus = s;
        }

        // ── Static Helpers ────────────────────────────────────────────────────

        /// <summary>
        /// オーバーキル比率 → 補正倍率を返す。
        /// 比率=0.0 → 0.50, 比率=0.5 → 0.80（ピーク）, 比率=2.0 → 0.10
        /// </summary>
        public static float CalcOverkillMultiplier(float ratio)
        {
            if (ratio <= 0.5f)
                return Mathf.Lerp(0.5f, 0.8f, ratio / 0.5f);
            else
                return Mathf.Lerp(0.8f, 0.1f, (ratio - 0.5f) / 1.5f);
        }

        /// <summary>レアリティ別ステータスブースト率（+2〜5%）</summary>
        private static float GetBoostRate(CharacterRarity rarity) => rarity switch
        {
            CharacterRarity.Common    => 0.02f,
            CharacterRarity.Uncommon  => 0.025f,
            CharacterRarity.Rare      => 0.03f,
            CharacterRarity.Epic      => 0.04f,
            CharacterRarity.Legendary => 0.045f,
            CharacterRarity.HyperRare => 0.05f,
            _                         => 0.02f,
        };
    }
}
