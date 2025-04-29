using Fusion.Addons.FSM;
using UnityEngine;
namespace Player
{
    public class IdleState : PlayerStateBehaviour
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
            
        }

        protected override void OnFixedUpdate()
        {
            //player.Move();
        }
        
        protected override void OnExitState()
        {
            
        }

        protected override void OnEnterStateRender()
        {
            Debug.Log("Idling...");
            // Animation
            player.anim.Play("Idle");
        }

        protected override void OnRender()
        {
            player.RotatePlayer();
        }

        protected override void OnExitStateRender()
        {

        }
    }
}