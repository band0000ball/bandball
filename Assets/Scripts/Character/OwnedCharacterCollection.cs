using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Character
{
    /// <summary>
    /// プレイヤーが所持するキャラクターのコレクション管理シングルトン。
    ///
    /// 上限 50 体。超過分は pendingCharacter に一時保持し、
    /// ステージ帰還時に削除操作を強制する。
    ///
    /// depends on: #12 PlayerWallet（レベル/AP管理）
    /// </summary>
    public class OwnedCharacterCollection : MonoBehaviour
    {
        public static OwnedCharacterCollection Instance { get; private set; }

        public const int MaxCapacity = 50;

        [SerializeField] private List<OwnedCharacterData> _characters = new();

        // ── 出撃メンバー ─────────────────────────────────────────
        /// <summary>プレイヤーが操作するキャラクター（1体）</summary>
        public OwnedCharacterData OperatingCharacter { get; private set; }

        /// <summary>パートナー枠（合計 2.0 スロット以内）</summary>
        public List<OwnedCharacterData> Partners { get; private set; } = new();

        /// <summary>ステージ中にコレクション満員で取得したキャラ（帰還時に処理必須）</summary>
        public OwnedCharacterData PendingCharacter { get; private set; }

        // ── Unity ────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ── コレクション操作 ──────────────────────────────────────
        public IReadOnlyList<OwnedCharacterData> Characters => _characters;

        public bool IsFull => _characters.Count >= MaxCapacity;

        /// <summary>キャラクターをコレクションに追加する。満員の場合 false を返す。</summary>
        public bool TryAdd(OwnedCharacterData data)
        {
            if (data == null || IsFull) return false;
            _characters.Add(data);
            return true;
        }

        /// <summary>コレクションからキャラクターを削除する（おわかれ・スクラップ時）</summary>
        public void Remove(OwnedCharacterData data)
        {
            if (data == null) return;
            _characters.Remove(data);

            if (OperatingCharacter == data) OperatingCharacter = null;
            Partners.Remove(data);
            if (PendingCharacter == data) PendingCharacter = null;
        }

        // ── ペンディング ──────────────────────────────────────────
        /// <summary>満員時の一時保持。帰還時に削除操作を強制すること。</summary>
        public void SetPending(OwnedCharacterData data) => PendingCharacter = data;

        public void ClearPending() => PendingCharacter = null;

        public bool HasPending => PendingCharacter != null;

        // ── 出撃設定 ──────────────────────────────────────────────
        /// <summary>操作キャラクターを設定する。</summary>
        public void SetOperator(OwnedCharacterData data)
        {
            OperatingCharacter = data;
            if (data != null) data.isActive = true;
        }

        /// <summary>パートナーを追加する。スロット上限 2.0 を超える場合 false を返す。</summary>
        public bool TryAddPartner(OwnedCharacterData data)
        {
            if (data == null) return false;
            float used = Partners.Sum(p => p.SlotSize);
            if (used + data.SlotSize > 2.0f) return false;
            Partners.Add(data);
            data.isActive = true;
            return true;
        }

        public void RemovePartner(OwnedCharacterData data)
        {
            if (data == null) return;
            Partners.Remove(data);
            data.isActive = false;
        }

        public void ClearPartners()
        {
            foreach (var p in Partners) p.isActive = false;
            Partners.Clear();
        }

        /// <summary>パートナースロットの現在使用量（0.0〜2.0）</summary>
        public float UsedPartnerSlots => Partners.Sum(p => p.SlotSize);

        // ── ステージ跨ぎリセット ──────────────────────────────────
        /// <summary>ステージ帰還時に全キャラの isDeadInStage をリセットする。</summary>
        public void ResetStageDeathFlags()
        {
            foreach (var c in _characters)
                c.isDeadInStage = false;
        }

        /// <summary>ステージ開始前に出撃メンバーをまとめて設定する。</summary>
        public void SetupDeployment(OwnedCharacterData operating, IEnumerable<OwnedCharacterData> partners)
        {
            // 前回の出撃フラグをクリア
            if (OperatingCharacter != null) OperatingCharacter.isActive = false;
            ClearPartners();

            SetOperator(operating);
            foreach (var p in partners)
                TryAddPartner(p);
        }
    }
}
