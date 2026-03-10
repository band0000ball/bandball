using Character.StateMachine;
using Commons;
using UnityEngine;

namespace Character.StateMachine.States
{
    public class JumpState : CharacterStateBase
    {
        public override bool CanGuard => false;

        protected override void OnUpdate()
        {
            if (Control.GetIsDead())
            {
                ForceChangeState<DeadState>();
                return;
            }

            if (Animator.GetCurrentAnimatorStateInfo(0).IsName("Damaged"))
            {
                ChangeState<DamagedState>();
                return;
            }

            if (!Movement.IsGrounded || Movement.IsJumping) return;
            if (Mathf.Abs(Movement.Velocity.x) > GameBalance.MOVEMENT_INPUT_THRESHOLD)
                ChangeState<WalkState>();
            else
                ChangeState<IdleState>();
        }
    }
}