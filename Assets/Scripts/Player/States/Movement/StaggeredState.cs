using Fusion.Addons.FSM;
using Fusion.Addons.SimpleKCC;
using UnityEngine;
namespace Player
{
    public class StaggeredState : PlayerStateBehaviour
    {
        protected override bool CanEnterState()
        {
            return Machine.ActiveState != player._staggeredState;
        }

        protected override bool CanExitState(PlayerStateBehaviour nextState)
        {
            return base.CanExitState(nextState);
        }

        protected override void OnEnterState()
        {
            player.ToggleHitboxes(false);
            player.moving = false;
            player.cm.collisions = false;
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
            player.cm.collisions = true;
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