using UnityEngine;
using Fusion.Addons.FSM;

namespace Enemy
{
    public class SlidingState : EnemyStateBehaviour
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
            Debug.Log("Sliding ...");

            enemy.rb.Rigidbody.linearVelocity = enemy.transform.forward * enemy.slideSpeed;
        }

        protected override void OnFixedUpdate()
        {
            if (Machine.StateTime > enemy.slideDuration)
            {
                Debug.Log("Slide Finished");
            }
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