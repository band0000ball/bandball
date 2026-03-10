using System;
using System.Collections.Generic;
using UnityEngine;

namespace Character.StateMachine
{
    /// <summary>
    /// キャラクターの状態を管理するステートマシン
    /// </summary>
    public class CharacterStateMachine
    {
        #region Private Fields

        private ICharacterState _currentState;
        private ICharacterState _previousState;
        private readonly Dictionary<Type, ICharacterState> _stateCache = new();

        #endregion

        #region Public Properties

        /// <summary>現在の状態</summary>
        public ICharacterState CurrentState => _currentState;

        /// <summary>前の状態</summary>
        public ICharacterState PreviousState => _previousState;

        /// <summary>現在の状態名</summary>
        public string CurrentStateName => _currentState?.Name ?? "None";

        /// <summary>CharacterControlへの参照</summary>
        public CharacterControl Control { get; }

        /// <summary>CharacterMovementへの参照</summary>
        public CharacterMovement Movement { get; }

        /// <summary>CharacterCombatへの参照</summary>
        public CharacterCombat Combat { get; }

        /// <summary>Animatorへの参照</summary>
        public Animator Animator { get; }

        #endregion

        #region State Capability Properties (ICharacterStats互換)

        /// <summary>現在の状態で移動可能か</summary>
        public bool CanMove => _currentState?.CanMove ?? true;

        /// <summary>現在の状態で攻撃可能か</summary>
        public bool CanAttack => _currentState?.CanAttack ?? true;

        /// <summary>現在の状態でガード可能か</summary>
        public bool CanGuard => _currentState?.CanGuard ?? true;

        /// <summary>現在の状態でダメージを受けるか</summary>
        public bool CanTakeDamage => _currentState?.CanTakeDamage ?? true;

        /// <summary>現在の状態で無敵か</summary>
        public bool IsInvincible => _currentState?.IsInvincible ?? false;

        #endregion

        #region Events

        /// <summary>状態が変更された時に発火するイベント</summary>
        public event Action<ICharacterState, ICharacterState> OnStateChanged;

        #endregion

        #region Constructor

        /// <summary>
        /// ステートマシンを初期化する
        /// </summary>
        /// <param name="control">CharacterControlへの参照</param>
        /// <param name="movement">CharacterMovementへの参照</param>
        /// <param name="combat">CharacterCombatへの参照</param>
        /// <param name="animator">Animatorへの参照</param>
        public CharacterStateMachine(
            CharacterControl control,
            CharacterMovement movement,
            CharacterCombat combat,
            Animator animator)
        {
            Control = control;
            Movement = movement;
            Combat = combat;
            Animator = animator;
        }

        #endregion

        #region State Management

        /// <summary>
        /// 状態を変更する（ジェネリック版）
        /// </summary>
        /// <typeparam name="T">遷移先の状態型</typeparam>
        /// <returns>遷移が成功した場合はtrue</returns>
        public bool ChangeState<T>() where T : class, ICharacterState, new()
        {
            var newState = GetOrCreateState<T>();
            return ChangeStateInternal(newState);
        }

        /// <summary>
        /// 状態を変更する（インスタンス版）
        /// </summary>
        /// <param name="newState">遷移先の状態</param>
        /// <returns>遷移が成功した場合はtrue</returns>
        public bool ChangeState(ICharacterState newState)
        {
            if (newState != null) return ChangeStateInternal(newState);
            Debug.LogWarning("CharacterStateMachine: Cannot change to null state");
            return false;

        }

        /// <summary>
        /// 強制的に状態を変更する（割り込みチェックをスキップ）
        /// </summary>
        /// <typeparam name="T">遷移先の状態型</typeparam>
        public void ForceChangeState<T>() where T : class, ICharacterState, new()
        {
            var newState = GetOrCreateState<T>();
            ForceChangeStateInternal(newState);
        }

        /// <summary>
        /// 前の状態に戻る
        /// </summary>
        /// <returns>遷移が成功した場合はtrue</returns>
        public bool RevertToPreviousState()
        {
            if (_previousState != null) return ChangeStateInternal(_previousState);
            Debug.LogWarning("CharacterStateMachine: No previous state to revert to");
            return false;

        }

        /// <summary>
        /// 指定した型の状態かどうかを判定する
        /// </summary>
        /// <typeparam name="T">判定する状態型</typeparam>
        /// <returns>現在の状態が指定した型の場合はtrue</returns>
        public bool IsInState<T>() where T : class, ICharacterState
        {
            return _currentState is T;
        }

        /// <summary>
        /// 現在の状態を指定した型として取得する
        /// </summary>
        /// <typeparam name="T">取得する状態型</typeparam>
        /// <returns>現在の状態（型が一致しない場合はnull）</returns>
        public T GetCurrentStateAs<T>() where T : class, ICharacterState
        {
            return _currentState as T;
        }

        #endregion

        #region Update Methods

        /// <summary>
        /// 毎フレーム呼び出す（Update）
        /// </summary>
        public void Update()
        {
            _currentState?.Update(this);
        }

        /// <summary>
        /// 物理更新時に呼び出す（FixedUpdate）
        /// </summary>
        public void FixedUpdate()
        {
            _currentState?.FixedUpdate(this);
        }

        #endregion

        #region Private Methods

        private T GetOrCreateState<T>() where T : class, ICharacterState, new()
        {
            var type = typeof(T);

            if (_stateCache.TryGetValue(type, out var cachedState))
            {
                return cachedState as T;
            }

            var newState = new T();
            _stateCache[type] = newState;
            return newState;
        }

        private bool ChangeStateInternal(ICharacterState newState)
        {
            // 同じ状態への遷移は無視
            if (_currentState == newState)
            {
                return false;
            }

            // 割り込みチェック
            if (_currentState != null && !_currentState.CanBeInterruptedBy(newState))
            {
                return false;
            }

            ExecuteStateTransition(newState);
            return true;
        }

        private void ForceChangeStateInternal(ICharacterState newState)
        {
            // 同じ状態への遷移は無視
            if (_currentState == newState)
            {
                return;
            }

            ExecuteStateTransition(newState);
        }

        private void ExecuteStateTransition(ICharacterState newState)
        {
            var oldState = _currentState;

            // 現在の状態を終了
            _currentState?.Exit(this);

            // 状態を更新
            _previousState = _currentState;
            _currentState = newState;

            // 新しい状態を開始
            _currentState.Enter(this);

            // イベント発火
            OnStateChanged?.Invoke(oldState, newState);

#if UNITY_EDITOR
            // ReSharper disable once Unity.PerformanceCriticalCodeInvocation
            // Debug.Log($"[StateMachine] {oldState?.Name ?? "None"} -> {newState.Name}");
#endif
        }

        #endregion
    }
}