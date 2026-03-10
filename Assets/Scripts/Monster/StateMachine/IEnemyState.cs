namespace Monster.StateMachine
{
    /// <summary>
    /// 敵 AI ステートのインターフェース。
    /// CharacterStateMachine の ICharacterState に対応する敵 AI 側の基底。
    /// </summary>
    public interface IEnemyState
    {
        string Name { get; }
        void Enter(EnemyStateMachine machine);
        void Update(EnemyStateMachine machine);
        void FixedUpdate(EnemyStateMachine machine);
        void Exit(EnemyStateMachine machine);
        bool CanBeInterruptedBy(IEnemyState newState);
    }
}
