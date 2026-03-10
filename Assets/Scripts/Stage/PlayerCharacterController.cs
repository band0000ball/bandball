using System.Collections.Generic;
using Character;
using Monster;
using UnityEngine;

namespace Stage
{
    /// <summary>
    /// ステージ内のプレイヤー操作キャラクターを管理するシングルトン。
    ///
    /// 責務:
    ///   - ステージ開始時に操作キャラをスポーン地点へ配置
    ///   - 操作キャラ死亡時にパートナーへ制御を引き継ぐ（死亡引き継ぎ）
    ///   - 全員死亡でゲームオーバーイベントを発火
    ///
    /// depends on: #34a (CharacterControl.OnCharacterDied / SetInputBase)
    /// </summary>
    public class PlayerCharacterController : MonoBehaviour
    {
        public static PlayerCharacterController Instance { get; private set; }

        [Header("スポーン")]
        [SerializeField] private PlayerSpawnPoint _spawnPoint;

        [Header("操作キャラクター")]
        [SerializeField] private CharacterControl _operatorControl;

        [Header("パートナー（優先度順）")]
        [SerializeField] private List<CharacterControl> _partnerControls = new();

        // ── Runtime state ─────────────────────────────────────────────────

        private CharacterControl _currentOperator;
        private readonly List<CharacterControl> _activePartners = new();

        /// <summary>現在プレイヤーが操作しているキャラクター</summary>
        public CharacterControl CurrentOperator => _currentOperator;

        /// <summary>現在のパートナーキャラクター一覧（UI 表示用）</summary>
        public IReadOnlyList<CharacterControl> ActivePartners => _activePartners;

        /// <summary>全員死亡（ゲームオーバー）時に発火。</summary>
        public event System.Action OnGameOver;

        // ── Unity ─────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            SetupForStage(_operatorControl, _partnerControls);
        }

        // ── Public API ────────────────────────────────────────────────────

        /// <summary>
        /// ステージ開始時に操作キャラとパートナーを登録してスポーン地点へ配置する。
        /// StageManager.StartStage() の後に呼び出すこと。
        /// </summary>
        public void SetupForStage(CharacterControl operatorControl, IEnumerable<CharacterControl> partners)
        {
            // 旧リスナーを解除
            if (_currentOperator != null)
                _currentOperator.OnCharacterDied -= OnOperatorDied;
            foreach (var p in _activePartners)
                p.OnCharacterDied -= OnPartnerDied;
            _activePartners.Clear();

            // 操作キャラ設定
            _currentOperator = operatorControl;
            if (_currentOperator != null)
            {
                _currentOperator.OnCharacterDied += OnOperatorDied;
                if (_spawnPoint != null)
                    _currentOperator.transform.position = _spawnPoint.Position;
            }

            // パートナー設定
            if (partners != null)
            {
                foreach (var p in partners)
                {
                    if (p == null || p == operatorControl) continue;
                    _activePartners.Add(p);
                    p.OnCharacterDied += OnPartnerDied;
                }
            }
        }

        /// <summary>
        /// 操作キャラクターを切り替える。
        /// newOperator の入力を PlayerInput に、旧オペレーターを AI（Monsters）に切り替える。
        /// </summary>
        public void SwitchOperator(CharacterControl newOperator)
        {
            if (newOperator == null || newOperator == _currentOperator) return;

            // 旧オペレーター → AI へ戻す
            if (_currentOperator != null)
            {
                _currentOperator.OnCharacterDied -= OnOperatorDied;
                SetInputToAI(_currentOperator);
            }

            // パートナーリストから除外
            _activePartners.Remove(newOperator);
            newOperator.OnCharacterDied -= OnPartnerDied;

            // 新オペレーター → PlayerInput へ切り替え
            SetInputToPlayer(newOperator);
            _currentOperator = newOperator;
            _currentOperator.OnCharacterDied += OnOperatorDied;
        }

        // ── Private ───────────────────────────────────────────────────────

        private void OnOperatorDied()
        {
            // 生存パートナーを探す
            CharacterControl next = null;
            foreach (var p in _activePartners)
            {
                if (!p.GetIsDead())
                {
                    next = p;
                    break;
                }
            }

            if (next == null)
            {
                OnGameOver?.Invoke();
                return;
            }

            SwitchOperator(next);
        }

        private void OnPartnerDied()
        {
            // パートナー死亡は記録のみ。全滅判定は OnOperatorDied で行う。
        }

        /// <summary>キャラクターの入力を PlayerInput に切り替える。</summary>
        private static void SetInputToPlayer(CharacterControl cc)
        {
            var pi = cc.GetComponent<Player.PlayerInput>();
            var ai = cc.GetComponent<Monsters>();
            if (ai != null) ai.enabled = false;
            if (pi != null) pi.enabled = true;
            cc.SetInputBase(pi);
        }

        /// <summary>キャラクターの入力を AI（Monsters）に切り替える。</summary>
        private static void SetInputToAI(CharacterControl cc)
        {
            var pi = cc.GetComponent<Player.PlayerInput>();
            var ai = cc.GetComponent<Monsters>();
            if (pi != null) pi.enabled = false;
            if (ai != null)
            {
                ai.enabled = true;
                cc.SetInputBase(ai);
            }
        }
    }
}
