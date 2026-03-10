using System;
using System.Collections.Generic;
using Character;
using Monster.StateMachine.States;
using UnityEngine;

namespace Monster.StateMachine
{
    /// <summary>
    /// 敵 AI のステートマシン。
    /// Monsters コンポーネントが所有し、Update/FixedUpdate を委譲する。
    ///
    /// 追加責務:
    ///   - 攻撃パターンの個別クールダウン管理
    ///   - HP 割合の計算（初期 HP 記録）
    ///   - ボスフェーズ遷移・エンレイジタイマー管理
    /// </summary>
    public class EnemyStateMachine
    {
        // ── 外部参照 ────────────────────────────────────────────────────────

        public CharacterControl Control { get; }
        public EnemyAIData AIData { get; }

        /// <summary>現在のターゲット（Monsters が毎フレーム更新する）</summary>
        public CharacterControl Target { get; set; }

        public IEnemyState CurrentState { get; private set; }

        // ── HP 管理 ──────────────────────────────────────────────────────────

        private readonly float _maxHealth;

        /// <summary>現在の HP 割合（0〜1）</summary>
        public float HpRatio => _maxHealth > 0
            ? Mathf.Clamp01(Control.GetHealth() / _maxHealth)
            : 0f;

        // ── パターンクールダウン ──────────────────────────────────────────────

        private readonly Dictionary<string, float> _patternCooldowns = new();

        // ── ボスフェーズ ──────────────────────────────────────────────────────

        /// <summary>現在のボスフェーズインデックス（0 = Phase1, 1 = Phase2, …）</summary>
        public int CurrentBossPhase { get; private set; } = -1;

        /// <summary>エンレイジ状態か</summary>
        public bool IsEnraged { get; private set; }

        private float _enrageElapsed;
        private bool _phaseTransitioning;

        /// <summary>フェーズ遷移による無敵タイマー</summary>
        public float PhaseInvincibleTimer { get; private set; }

        /// <summary>現在の攻撃力倍率（フェーズ・エンレイジ考慮）</summary>
        public float AttackMultiplier => IsEnraged ? AIData.enrageAttackMultiplier : GetPhaseMultiplier(true);

        /// <summary>現在の移動速度倍率（フェーズ・エンレイジ考慮）</summary>
        public float SpeedMultiplier => IsEnraged ? AIData.enrageSpeedMultiplier : GetPhaseMultiplier(false);

        // ── ステートキャッシュ ────────────────────────────────────────────────

        private readonly Dictionary<Type, IEnemyState> _stateCache = new();

        // ── コンストラクタ ────────────────────────────────────────────────────

        public EnemyStateMachine(CharacterControl control, EnemyAIData aiData)
        {
            Control = control;
            AIData = aiData;
            _maxHealth = control.GetHealth();
        }

        // ── 更新 ──────────────────────────────────────────────────────────────

        public void Update()
        {
            // パターンクールダウンをティック
            TickPatternCooldowns();

            // フェーズ無敵タイマー
            if (PhaseInvincibleTimer > 0)
                PhaseInvincibleTimer -= Time.deltaTime;

            // ボスフェーズチェック
            if (AIData.isBoss)
                CheckBossPhases();

            CurrentState?.Update(this);
        }

        public void FixedUpdate() => CurrentState?.FixedUpdate(this);

        // ── ステート遷移 ──────────────────────────────────────────────────────

        public bool ChangeState<T>() where T : class, IEnemyState, new()
        {
            var next = GetOrCreate<T>();
            if (CurrentState != null && !CurrentState.CanBeInterruptedBy(next)) return false;
            Transition(next);
            return true;
        }

        public void ForceChangeState<T>() where T : class, IEnemyState, new()
            => Transition(GetOrCreate<T>());

        // ── パターンクールダウン ──────────────────────────────────────────────

        public bool IsPatternReady(string patternName)
            => !_patternCooldowns.ContainsKey(patternName);

        public void StartPatternCooldown(string patternName, float cooldown)
            => _patternCooldowns[patternName] = cooldown;

        // ── 現在フェーズの攻撃パターン取得 ────────────────────────────────────

        /// <summary>
        /// 現在フェーズの全攻撃パターン（基本 + フェーズ追加分）を返す。
        /// </summary>
        public AttackPattern[] GetCurrentPatterns()
        {
            var base_ = AIData.attackPatterns ?? new AttackPattern[0];
            if (!AIData.isBoss || CurrentBossPhase < 0 || CurrentBossPhase >= AIData.bossPhases.Length)
                return base_;

            var phase = AIData.bossPhases[CurrentBossPhase];
            var additional = phase.additionalPatterns ?? new AttackPattern[0];

            var merged = new AttackPattern[base_.Length + additional.Length];
            base_.CopyTo(merged, 0);
            additional.CopyTo(merged, base_.Length);
            return merged;
        }

        // ── Private ───────────────────────────────────────────────────────────

        private void Transition(IEnemyState next)
        {
            CurrentState?.Exit(this);
            CurrentState = next;
            CurrentState.Enter(this);
        }

        private T GetOrCreate<T>() where T : class, IEnemyState, new()
        {
            if (!_stateCache.TryGetValue(typeof(T), out var state))
            {
                state = new T();
                _stateCache[typeof(T)] = state;
            }
            return (T)state;
        }

        private void TickPatternCooldowns()
        {
            var keys = new List<string>(_patternCooldowns.Keys);
            foreach (var k in keys)
            {
                _patternCooldowns[k] -= Time.deltaTime;
                if (_patternCooldowns[k] <= 0)
                    _patternCooldowns.Remove(k);
            }
        }

        private void CheckBossPhases()
        {
            if (_phaseTransitioning) return;

            // フェーズ昇順でチェック（Phase0=HP70%, Phase1=HP40%, Phase2=HP10%…）
            for (int i = 0; i < AIData.bossPhases.Length; i++)
            {
                if (HpRatio <= AIData.bossPhases[i].hpThreshold && CurrentBossPhase < i)
                {
                    TransitionToPhase(i);
                    return;
                }
            }

            // エンレイジタイマー
            if (!IsEnraged)
            {
                _enrageElapsed += Time.deltaTime;
                if (_enrageElapsed >= AIData.enrageTimer)
                    ActivateEnrage();
            }
        }

        private void TransitionToPhase(int phaseIndex)
        {
            CurrentBossPhase = phaseIndex;
            _phaseTransitioning = true;
            PhaseInvincibleTimer = AIData.bossPhases[phaseIndex].transitionInvincibleDuration;
            // 無敵タイマーが切れたら _phaseTransitioning をリセット
            // （EnemyPhaseTransitionState または Update で管理する）
            _phaseTransitioning = false;
        }

        private void ActivateEnrage()
        {
            IsEnraged = true;
        }

        private float GetPhaseMultiplier(bool isAttack)
        {
            if (!AIData.isBoss || CurrentBossPhase < 0 || CurrentBossPhase >= AIData.bossPhases.Length)
                return 1f;
            var phase = AIData.bossPhases[CurrentBossPhase];
            return isAttack ? phase.attackMultiplier : phase.speedMultiplier;
        }
    }
}
