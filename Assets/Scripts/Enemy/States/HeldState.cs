using UnityEngine;
using Fusion.Addons.FSM;

namespace Enemy
{
    public class HeldState : EnemyStateBehaviour
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
            // Doesnt work
            Debug.Log("Held start");
            enemy.rb.Rigidbody.useGravity = false;
            enemy.body.transform.SetParent(enemy.holdPos.transform);
            enemy.body.transform.position = enemy.holdPos.transform.position;
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