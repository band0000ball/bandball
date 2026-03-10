using System;
using System.Collections.Generic;
using System.Linq;
using Character;
using Commons;
using Manager;
using Monster.StateMachine;
using Monster.StateMachine.States;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Monster
{
    /// <summary>
    /// 敵キャラクターの AI コンポーネント。InputBase を継承し、
    /// CharacterControl の _input として機能する。
    ///
    /// EnemyStateMachine を所有し、毎フレームの移動/攻撃/ガード入力を
    /// ステートクラスに委譲する。
    ///
    /// ヘイトシステムによってターゲットを決定し、EnemyStateMachine.Target へ渡す。
    /// </summary>
    public class Monsters : InputBase
    {
        // ── Inspector ─────────────────────────────────────────────────────────

        [Header("AI データ")]
        [SerializeField] private EnemyAIData _aiData;

        [Header("ヘイト")]
        [SerializeField] private UniqueIDManager uniqueIDManager;

        // ── Private: ステートマシン ───────────────────────────────────────────

        private EnemyStateMachine _stateMachine;
        private CharacterControl _characterControl;
        private Rigidbody _rigidbody;

        // ── Private: ヘイト管理 ───────────────────────────────────────────────

        private int _hateVersion;
        private float[] _hate;
        private List<int> _hateHash;
        private CharacterControl[] _controls;
        private CharacterControl _hateTarget;
        private int _hateTargetIdx;
        private float _positionDiffX;

        // ── Unity ─────────────────────────────────────────────────────────────

        private void Awake()
        {
            _characterControl = GetComponent<CharacterControl>();
            _rigidbody = GetComponent<Rigidbody>();
            _positionDiffX = 5f;
        }

        private void Start()
        {
            // ヘイト初期化
            _controls = FindObjectsByType<CharacterControl>(FindObjectsSortMode.None);
            _hate = new float[_controls.Length];
            _hateHash = _controls.Select(x => x.GetUniqueId()).ToList();

            // ステートマシン初期化
            if (_aiData == null)
            {
                Debug.LogWarning($"[Monsters] {name}: EnemyAIData が未設定です。デフォルト設定で動作します。", this);
                _aiData = ScriptableObject.CreateInstance<EnemyAIData>();
            }

            _stateMachine = new EnemyStateMachine(_characterControl, _aiData);
            _stateMachine.ForceChangeState<EnemyIdleState>();

            // スキル初期化（旧コードより継承）
            foreach (var isAttack in _characterControl.isAttackItems.Select((x, i) => new { x, i }))
            {
                if (isAttack.x) _characterControl.SwapSkillActive(isAttack.i);
            }
        }

        private void Update()
        {
            // ヘイトバージョン同期
            if (uniqueIDManager != null)
            {
                int uidmVersion = uniqueIDManager.GetVersion();
                if (_hateVersion < uidmVersion)
                {
                    ResetHate();
                    _hateVersion++;
                }
            }

            // ターゲット更新
            UpdateHateTarget();
            if (_stateMachine != null)
                _stateMachine.Target = _hateTarget;

            // キャラクター向きの更新（ターゲット方向を向く）
            UpdateFacing();

            // ステートマシン更新
            _stateMachine?.Update();
        }

        private void FixedUpdate()
        {
            _stateMachine?.FixedUpdate();
        }

        // ── Hate System ───────────────────────────────────────────────────────

        private void UpdateHateTarget()
        {
            if (_controls == null || _controls.Length == 0) return;

            int idx = Array.IndexOf(_hate, _hate.Max());

            // 自分自身はターゲットにしない
            if (_controls[idx].name == name)
            {
                idx = (idx + 1) % _controls.Length;
            }

            if (idx != _hateTargetIdx || _hateTarget == null)
            {
                _hateTargetIdx = idx;
                _hateTarget = _controls[idx];
            }

            if (_hateTarget != null && _hateTarget.GetHealth() > 0)
                _positionDiffX = _hateTarget.GetRigidPositionDiffX(_rigidbody.position);
        }

        private void UpdateFacing()
        {
            // 移動していないときはターゲット方向を向く
            if (Mathf.Approximately(move.x, 0f))
            {
                _rigidbody.rotation = Quaternion.AngleAxis(
                    _positionDiffX > 0f ? 180f : 0f, Vector3.up);
            }
        }

        // ── Collision (Hate へのダメージ加算) ────────────────────────────────

        private void OnCollisionEnter(Collision collision)
        {
            var effect = collision.gameObject.GetComponent<Skill.Component.SkillComponent>();
            if (collision.gameObject.layer != LayerMask.NameToLayer("Effect") || effect == null) return;

            var parentControl = effect.parent?.GetComponent<CharacterControl>();
            if (parentControl == null) return;

            int parentId = parentControl.GetUniqueId();
            int hashIdx = _hateHash.IndexOf(parentId);
            if (hashIdx >= 0 && hashIdx < _hate.Length)
                _hate[hashIdx] += 1f;
        }

        // ── Private Helpers ───────────────────────────────────────────────────

        private void ResetHate()
        {
            _controls = FindObjectsByType<CharacterControl>(FindObjectsSortMode.None);
            float[] newHate = new float[_controls.Length];
            List<int> newHateHash = _controls.Select(x => x.GetUniqueId()).ToList();

            foreach (var ctl in _controls)
            {
                int oldIdx = _hateHash.IndexOf(ctl.GetUniqueId());
                int newIdx = newHateHash.IndexOf(ctl.GetUniqueId());
                if (oldIdx >= 0 && newIdx >= 0)
                    newHate[newIdx] = _hate[oldIdx];
            }

            _hate = newHate;
            _hateHash = newHateHash;
        }
    }
}
