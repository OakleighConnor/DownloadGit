using Fusion.Addons.FSM;
using NUnit.Framework;
using UnityEngine;
namespace Player
{
    public class HoldingState : PlayerStateBehaviour
    {
        protected override bool CanEnterState()
        {
            return player.CheckForPickupTarget();
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
            Debug.Log("Holding ...");

            if(player.CheckForThrow())
            {
                //Machine.TryDeactivateState(StateId);
            }
        }
        
        protected override void OnExitState()
        {
            player.ThrowObject();
        }

        protected override void OnEnterStateRender()
        {
            
        }

        protected override void OnRender()
        {
            
        }

        protected override void OnExitStateRender()
        {
            player.am.PlaySFX(player.am.playerThrow);
        }
    }
}