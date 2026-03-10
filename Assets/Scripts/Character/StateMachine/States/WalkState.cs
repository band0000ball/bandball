using Character.StateMachine;
using Commons;
using UnityEngine;

namespace Character.StateMachine.States
{
    public class WalkState : CharacterStateBase
    {
        protected override void OnUpdate()
        {
            if (Control.GetIsDead())
            {
                ForceChangeState<DeadState>();
                return;
            }

            var animState = Animator.GetCurrentAnimatorStateInfo(0);

            if (animState.IsName("Damaged"))
            {
                ChangeState<DamagedState>();
                return;
            }

            if (Control.GetRoll())
            {
                ChangeState<RollingState>();
                return;
            }

            if (Control.GetChargeReady())
            {
                ChangeState<ChargeState>();
                return;
            }

            if (Control.GetCrouching())
            {
                ChangeState<GuardState>();
                return;
            }

            if (animState.IsName("Attack1"))
            {
                ChangeState<AttackState>();
                return;
            }

            if (Movement.IsJumping)
            {
                ChangeState<JumpState>();
                return;
            }

            if (Mathf.Abs(Movement.Velocity.x) <= GameBalance.MOVEMENT_INPUT_THRESHOLD)
            {
                ChangeState<IdleState>();
            }
        }
    }
}