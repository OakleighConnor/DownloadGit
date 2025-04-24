using Fusion.Addons.FSM;
using UnityEngine;
namespace Player
{
    public class StaggeredState : PlayerStateBehaviour
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
            player.ToggleHitboxes(false);
            player.moving = false;
        }

        protected override void OnFixedUpdate()
        {
            if (Machine.StateTime > 4.4f)
            {
                // Stagger Finished
                Machine.TryDeactivateState(StateId);
            }
        }
        
        protected override void OnExitState()
        {
            player.ToggleHitboxes(true);
            player.moving = true;
        }

        protected override void OnEnterStateRender()
        {
            Debug.Log("Staggered...");
            // Animation
            player.anim.Play("Stagger");
        }

        protected override void OnRender()
        {
            
        }

        protected override void OnExitStateRender()
        {

        }
    }
}