namespace Character.StateMachine
{
    /// <summary>
    /// キャラクター状態の基底クラス
    /// 各状態クラスはこのクラスを継承して実装する
    /// </summary>
    public abstract class CharacterStateBase : ICharacterState
    {
        #region ICharacterState Properties

        /// <summary>状態の識別名（クラス名から自動生成）</summary>
        public virtual string Name => GetType().Name.Replace("State", "");

        /// <summary>移動可能か（デフォルト: true）</summary>
        public virtual bool CanMove => true;

        /// <summary>攻撃可能か（デフォルト: true）</summary>
        public virtual bool CanAttack => true;

        /// <summary>ガード可能か（デフォルト: true）</summary>
        public virtual bool CanGuard => true;

        /// <summary>ダメージを受けるか（デフォルト: true）</summary>
        public virtual bool CanTakeDamage => true;

        /// <summary>無敵状態か（デフォルト: false）</summary>
        public virtual bool IsInvincible => false;

        #endregion

        #region Protected Properties

        /// <summary>ステートマシンへの参照（Enter時に設定される）</summary>
        protected CharacterStateMachine Machine { get; private set; }

        /// <summary>CharacterControlへのショートカット</summary>
        protected CharacterControl Control => Machine?.Control;

        /// <summary>CharacterMovementへのショートカット</summary>
        protected CharacterMovement Movement => Machine?.Movement;

        /// <summary>CharacterCombatへのショートカット</summary>
        protected CharacterCombat Combat => Machine?.Combat;

        /// <summary>Animatorへのショートカット</summary>
        protected UnityEngine.Animator Animator => Machine?.Animator;

        #endregion

        #region ICharacterState Lifecycle Methods

        /// <summary>
        /// 状態に入った時に呼ばれる（内部処理）
        /// </summary>
        void ICharacterState.Enter(CharacterStateMachine machine)
        {
            Machine = machine;
            OnEnter();
        }

        /// <summary>
        /// 毎フレーム呼ばれる（内部処理）
        /// </summary>
        void ICharacterState.Update(CharacterStateMachine machine)
        {
            OnUpdate();
        }

        /// <summary>
        /// 物理更新時に呼ばれる（内部処理）
        /// </summary>
        void ICharacterState.FixedUpdate(CharacterStateMachine machine)
        {
            OnFixedUpdate();
        }

        /// <summary>
        /// 状態から出る時に呼ばれる（内部処理）
        /// </summary>
        void ICharacterState.Exit(CharacterStateMachine machine)
        {
            OnExit();
        }

        #endregion

        #region Protected Virtual Methods (Override in derived classes)

        /// <summary>
        /// 状態に入った時の処理
        /// </summary>
        protected virtual void OnEnter() { }

        /// <summary>
        /// 毎フレームの更新処理
        /// </summary>
        protected virtual void OnUpdate() { }

        /// <summary>
        /// 物理更新処理
        /// </summary>
        protected virtual void OnFixedUpdate() { }

        /// <summary>
        /// 状態から出る時の処理
        /// </summary>
        protected virtual void OnExit() { }

        #endregion

        #region ICharacterState Transition Methods

        /// <summary>
        /// 指定した状態による割り込みが可能か（デフォルト: 全て許可）
        /// </summary>
        public virtual bool CanBeInterruptedBy(ICharacterState newState)
        {
            return true;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 状態を変更する
        /// </summary>
        /// <typeparam name="T">遷移先の状態型</typeparam>
        /// <returns>遷移が成功した場合はtrue</returns>
        protected bool ChangeState<T>() where T : class, ICharacterState, new()
        {
            return Machine?.ChangeState<T>() ?? false;
        }

        /// <summary>
        /// 強制的に状態を変更する
        /// </summary>
        /// <typeparam name="T">遷移先の状態型</typeparam>
        protected void ForceChangeState<T>() where T : class, ICharacterState, new()
        {
            Machine?.ForceChangeState<T>();
        }

        #endregion
    }
}