using UnityEngine;
using Fusion.Addons.FSM;

namespace Enemy
{
    public class TurningState : EnemyStateBehaviour
    {
        protected override bool CanEnterState()
        {
            return base.CanEnterState();
        }

        protected override bool CanExitState(EnemyStateBehaviour nextState)
        {
            return base.CanExitState(nextState);
        }

        protected override void OnEnterState()
        {
            
        }

        protected override void OnFixedUpdate()
        {
            
        }
        
        protected override void OnExitState()
        {
            
        }

        protected override void OnEnterStateRender()
        {
            
        }

        protected override void OnRender()
        {
            
        }

        protected override void OnExitStateRender()
        {

        }
    }
}