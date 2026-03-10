using UnityEngine;

namespace Stage
{
    /// <summary>
    /// ステージ難易度を管理するシングルトン。
    /// ステージ選択時にのみ SetDifficulty() を呼ぶこと。
    /// ステージ開始後（LockForStage() 以降）は変更不可。
    ///
    /// 敵レベル補正: Monsters.cs が EnemyLevelBonus を参照して実効レベルに加算する。
    /// ドロップ倍率: DropHandler が Current.currencyDropMultiplier 等を参照する。
    ///
    /// depends on: #32 StageManager（LockForStage/UnlockAfterStage 連携）
    /// </summary>
    public class DifficultyManager : MonoBehaviour
    {
        public static DifficultyManager Instance { get; private set; }

        /// <summary>難易度変更時に発火。</summary>
        public static event System.Action<DifficultySettings> OnDifficultyChanged;

        private DifficultySettings _current;
        private bool _locked;

        // ── Properties ────────────────────────────────────────────────────────

        /// <summary>現在の難易度設定。</summary>
        public DifficultySettings Current => _current;

        /// <summary>ステージ攻略中は true（難易度変更不可）。</summary>
        public bool IsLocked => _locked;

        // ── Unity ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _current = DifficultySettings.CreateNormal();
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// 難易度を変更する。ステージ攻略中（IsLocked == true）は無視される。
        /// </summary>
        public bool SetDifficulty(StageDifficulty difficulty)
        {
            if (_locked)
            {
                Debug.LogWarning("[DifficultyManager] ステージ攻略中は難易度を変更できません。");
                return false;
            }

            _current = difficulty switch
            {
                StageDifficulty.Normal => DifficultySettings.CreateNormal(),
                StageDifficulty.Hard   => DifficultySettings.CreateHard(),
                StageDifficulty.Ultra  => DifficultySettings.CreateUltra(),
                _                      => DifficultySettings.CreateNormal(),
            };

            OnDifficultyChanged?.Invoke(_current);
            return true;
        }

        /// <summary>ステージ開始時に呼ぶ。難易度変更をロックする。</summary>
        public void LockForStage()  => _locked = true;

        /// <summary>ステージ終了時（クリア/リタイア）に呼ぶ。ロック解除。</summary>
        public void UnlockAfterStage() => _locked = false;

        // ── Convenience ───────────────────────────────────────────────────────

        public int   EnemyLevelBonus          => _current.enemyLevelBonus;
        public float CurrencyDropMultiplier   => _current.currencyDropMultiplier;
        public float ItemDropRateMultiplier   => _current.itemDropRateMultiplier;
        public float RareItemRateMultiplier   => _current.rareItemRateMultiplier;
    }
}
