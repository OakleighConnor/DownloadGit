using Fusion.Addons.FSM;
using UnityEngine;
namespace Player
{
    public class FallingState : PlayerStateBehaviour
    {
        protected override bool CanEnterState()
        {
            return base.CanEnterState();
        }

        /*protected override bool CanExitState(PlayerStateBehaviour nextState)
        {
            return player.cc.Grounded || nextState == player._jumpState;
        }*/
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
            Debug.Log("Falling...");
            // Animation
            player.anim.Play("Fall");
        }

        protected override void OnRender()
        {
            
        }

        protected override void OnExitStateRender()
        {

        }
    }
}