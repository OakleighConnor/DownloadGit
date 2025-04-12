using Fusion.Addons.FSM;
using UnityEngine;
namespace Player
{
    public class IdleState : PlayerStateBehaviour
    {
        protected override bool CanEnterState()
        {
            return player.cc.Velocity.x <= 0.1f;
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
            /*if (Machine.StateTime > 1f)
            {
                // Attack finished, deactivate
                Machine.TryDeactivateState(StateId);
            }*/

            player.Move();
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
            
        }

        protected override void OnExitStateRender()
        {

        }
    }
}