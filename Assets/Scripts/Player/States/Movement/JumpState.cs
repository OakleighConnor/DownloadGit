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

        /*protected override bool CanExitState(PlayerStateBehaviour nextState)
        {
            if (Machine.StateTime < 0.1f) return false;
            //return player.cc.Grounded || nextState == player._fallingState;
            return base.CanExitState(sta)
        }*/

        protected override bool CanExitState(PlayerStateBehaviour nextState)
        {
            return base.CanExitState(nextState);
        }

        protected override void OnEnterState()
        {
            //if (Object.HasStateAuthority) player.cc.Velocity = new Vector3(player.cc.Velocity.x, 20, player.cc.Velocity.z);
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
            Debug.Log("Jumping...");
            // Animation
            player.anim.Play("Jump");
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