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
            enemy.transform.SetParent(enemy.holdPos.transform);
        }

        protected override void OnFixedUpdate()
        {
            
        }
        
        protected override void OnExitState()
        {
            Debug.Log("Held over");
            enemy.rb.Rigidbody.useGravity = true;
            enemy.transform.parent = null;
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