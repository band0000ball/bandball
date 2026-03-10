using System.Collections.Generic;
using Character;
using UnityEngine;

namespace Stage
{
    /// <summary>
    /// ステージ進行全体を管理するシングルトン。DontDestroyOnLoad でシーン間を跨いで維持される。
    /// GameSceneManager のシーン遷移前に ClearStage() が呼ばれてリセットされる。
    ///
    /// 主な責務:
    ///   - ステージ開始・終了の管理
    ///   - 敵の登録と死亡追跡（KillAllEnemies / DefeatBoss 条件判定）
    ///   - チェックポイントの記録（リスポーン地点更新）
    ///   - Survival タイマーの管理
    ///   - クリア条件成立時に OnStageClear イベントを発火
    /// </summary>
    public class StageManager : MonoBehaviour
    {
        public static StageManager Instance { get; private set; }

        // ── 現在のステージ状態 ──────────────────────────────────────────

        /// <summary>現在プレイ中のステージデータ</summary>
        public StageData CurrentStage { get; private set; }

        /// <summary>到達済みの最大チェックポイントインデックス（-1 = 未到達）</summary>
        public int CurrentCheckpointIndex { get; private set; } = -1;

        /// <summary>最後に到達したチェックポイントのワールド座標（リスポーン地点）</summary>
        public Vector3 RespawnPosition { get; private set; }

        /// <summary>ステージクリア済みか</summary>
        public bool IsStageClear { get; private set; }

        // ── 敵追跡 ──────────────────────────────────────────────────────

        private readonly HashSet<GameObject> _activeEnemies = new();
        private bool _anyEnemyRegistered;
        private int _totalEnemiesKilled;

        // ── Survival タイマー ────────────────────────────────────────────

        private float _survivalTimer;
        private bool _isRunning;

        // ── イベント ─────────────────────────────────────────────────────

        /// <summary>クリア条件成立時に発火。ResultUI 等を購読させる。</summary>
        public event System.Action OnStageClear;

        /// <summary>チェックポイント通過時に発火。引数はチェックポイントのインデックス。</summary>
        public event System.Action<int> OnCheckpointReached;

        // ── Unity ライフサイクル ──────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (!_isRunning || CurrentStage == null) return;

            if (CurrentStage.clearCondition == ClearCondition.Survival)
            {
                _survivalTimer -= Time.deltaTime;
                if (_survivalTimer <= 0f)
                    TriggerStageClear();
            }
        }

        // ── Public API ────────────────────────────────────────────────────

        /// <summary>
        /// ステージを開始する。シーン遷移後・ステージ選択後に呼び出す。
        /// </summary>
        public void StartStage(StageData stageData)
        {
            CurrentStage       = stageData;
            CurrentCheckpointIndex = -1;
            IsStageClear       = false;
            _activeEnemies.Clear();
            _anyEnemyRegistered = false;
            _totalEnemiesKilled = 0;
            _isRunning         = true;

            if (stageData.clearCondition == ClearCondition.Survival)
                _survivalTimer = stageData.survivalTime;

            // TODO: #38 セーブデータのステージ開始レコード記録
            // TODO: BGM 再生
        }

        /// <summary>
        /// スポーンした敵を登録し、死亡時に自動通知されるよう購読する。
        /// </summary>
        /// <param name="enemy">スポーンした敵の GameObject</param>
        /// <param name="isBoss">ボス判定（true の場合 DefeatBoss 条件に使用）</param>
        public void RegisterEnemy(GameObject enemy, bool isBoss = false)
        {
            if (enemy == null) return;

            _activeEnemies.Add(enemy);
            _anyEnemyRegistered = true;

            // CharacterCombat.OnDeath を購読して死亡時に通知を受ける
            var combat = enemy.GetComponentInChildren<CharacterCombat>();
            if (combat != null)
            {
                bool bossFlag = isBoss;
                combat.OnDeath += () =>
                {
                    if (bossFlag)
                        NotifyBossDefeated();
                    else
                        NotifyEnemyDeath(enemy);
                };
            }
        }

        /// <summary>
        /// ボス以外の敵が死亡したことを通知する。通常は RegisterEnemy の内部購読から呼ばれる。
        /// </summary>
        public void NotifyEnemyDeath(GameObject enemy)
        {
            if (!_activeEnemies.Remove(enemy)) return;
            _totalEnemiesKilled++;

            if (CurrentStage?.clearCondition == ClearCondition.KillAllEnemies
                && _anyEnemyRegistered
                && _activeEnemies.Count == 0)
            {
                TriggerStageClear();
            }
        }

        /// <summary>
        /// ボスが倒されたことを通知する。DefeatBoss 条件の判定に使用。
        /// </summary>
        public void NotifyBossDefeated()
        {
            if (CurrentStage?.clearCondition == ClearCondition.DefeatBoss)
                TriggerStageClear();
        }

        /// <summary>
        /// ゴール地点への到達を通知する。GoalTrigger から呼ばれる。
        /// </summary>
        public void NotifyGoalReached()
        {
            if (CurrentStage?.clearCondition == ClearCondition.ReachGoal)
                TriggerStageClear();
        }

        /// <summary>
        /// チェックポイント通過を記録する。CheckpointTrigger から呼ばれる。
        /// インデックスが現在より大きい場合のみ更新（後退防止）。
        /// </summary>
        public void RegisterCheckpoint(int index, Vector3 worldPosition)
        {
            if (index <= CurrentCheckpointIndex) return;

            CurrentCheckpointIndex = index;
            RespawnPosition        = worldPosition;
            OnCheckpointReached?.Invoke(index);

            // TODO: #38 PlayFab / ES3 にチェックポイントデータをセーブ
        }

        /// <summary>
        /// ステージ状態をリセットする。GameSceneManager のシーン遷移前に呼ばれる。
        /// </summary>
        public void ClearStage()
        {
            CurrentStage        = null;
            CurrentCheckpointIndex = -1;
            IsStageClear        = false;
            _isRunning          = false;
            _activeEnemies.Clear();
            _anyEnemyRegistered = false;
            _totalEnemiesKilled = 0;
        }

        // ── Private ──────────────────────────────────────────────────────

        private void TriggerStageClear()
        {
            if (IsStageClear) return;

            IsStageClear = true;
            _isRunning   = false;
            OnStageClear?.Invoke();

            // TODO: #38 クリアデータをセーブ
            // TODO: リザルトUI オーバーレイを表示
        }
    }
}
