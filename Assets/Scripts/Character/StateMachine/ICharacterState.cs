namespace Character.StateMachine
{
    /// <summary>
    /// キャラクター状態の基本インターフェース
    /// 全ての状態クラスはこのインターフェースを実装する
    /// </summary>
    public interface ICharacterState
    {
        #region Properties

        /// <summary>状態の識別名</summary>
        string Name { get; }

        /// <summary>移動可能か</summary>
        bool CanMove { get; }

        /// <summary>攻撃可能か</summary>
        bool CanAttack { get; }

        /// <summary>ガード可能か</summary>
        bool CanGuard { get; }

        /// <summary>ダメージを受けるか</summary>
        bool CanTakeDamage { get; }

        /// <summary>無敵状態か</summary>
        bool IsInvincible { get; }

        #endregion

        #region Lifecycle Methods

        /// <summary>
        /// 状態に入った時に呼ばれる
        /// </summary>
        /// <param name="machine">ステートマシンへの参照</param>
        void Enter(CharacterStateMachine machine);

        /// <summary>
        /// 毎フレーム呼ばれる（Update）
        /// </summary>
        /// <param name="machine">ステートマシンへの参照</param>
        void Update(CharacterStateMachine machine);

        /// <summary>
        /// 物理更新時に呼ばれる（FixedUpdate）
        /// </summary>
        /// <param name="machine">ステートマシンへの参照</param>
        void FixedUpdate(CharacterStateMachine machine);

        /// <summary>
        /// 状態から出る時に呼ばれる
        /// </summary>
        /// <param name="machine">ステートマシンへの参照</param>
        void Exit(CharacterStateMachine machine);

        #endregion

        #region Transition Methods

        /// <summary>
        /// 指定した状態による割り込みが可能か
        /// </summary>
        /// <param name="newState">遷移先の状態</param>
        /// <returns>割り込み可能な場合はtrue</returns>
        bool CanBeInterruptedBy(ICharacterState newState);

        #endregion
    }
}