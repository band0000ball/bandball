using Character.StateMachine;

namespace Character.StateMachine.States
{
    /// <summary>
    /// 死亡状態。終端状態のため遷移不可・全アクション不可。
    /// CanTakeDamage=falseで二重ダメージを防ぐ。
    /// </summary>
    public class DeadState : CharacterStateBase
    {
        public override bool CanMove => false;
        public override bool CanAttack => false;
        public override bool CanGuard => false;
        public override bool CanTakeDamage => false;

        public override bool CanBeInterruptedBy(ICharacterState newState) => false;

        protected override void OnEnter()
        {
            Control.NotifyDeath();
        }
    }
}