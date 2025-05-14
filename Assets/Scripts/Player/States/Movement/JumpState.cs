using Fusion.Addons.FSM;
using UnityEngine;
namespace Player
{
    public class JumpState : PlayerStateBehaviour
    {
        protected override bool CanEnterState()
        {
            return base.CanEnterState();
        }

        protected override bool CanExitState(PlayerStateBehaviour nextState)
        {
            return base.CanExitState(nextState);
        }

        protected override void OnEnterState()
        {
            player.Jump();
        }

        protected override void OnFixedUpdate()
        {
            
        }
        
        protected override void OnExitState()
        {
            
        }

        protected override void OnEnterStateRender()
        {
            Debug.Log("Jumping...");
            // Animation
            player.anim.Play("Jump");
        }

        protected override void OnRender()
        {
            player.UpdatePlayerDirection();
        }

        protected override void OnExitStateRender()
        {

        }
    }
}