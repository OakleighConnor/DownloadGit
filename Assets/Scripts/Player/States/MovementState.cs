using Fusion.Addons.FSM;
using UnityEngine;
namespace Player
{
    public class MovementState : PlayerStateBehaviour
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
            /*
            playerScript.dir.Normalize();
            cc.Move(cc.maxSpeed * playerScript.dir * 1000 * Runner.DeltaTime);
            */

            //player.Move();
        }
        
        protected override void OnExitState()
        {
            
        }

        protected override void OnEnterStateRender()
        {
            //Debug.Log("Moving...");
        }

        protected override void OnRender()
        {
            // Animation
            if(player.dir.x < 0)
            {
                player.anim.Play("Run Left");
            }
            else
            {
                player.anim.Play("Run Right");
            }
        }

        protected override void OnExitStateRender()
        {

        }
    }
}