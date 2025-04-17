using UnityEngine;
using Fusion.Addons.FSM;

namespace Enemy
{
    public class StaggeredState : EnemyStateBehaviour
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
            enemy.rb.Rigidbody.linearVelocity = new Vector3(0,0,0);
            enemy.transform.Rotate(0,0,180);
        }

        protected override void OnFixedUpdate()
        {
            if (Machine.StateTime > 3f)
            {
                // Attack finished, deactivate
                Machine.TryDeactivateState(StateId);
            }
        }
        
        protected override void OnExitState()
        {
            enemy.transform.rotation = Quaternion.Euler(0, 90, 0);
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