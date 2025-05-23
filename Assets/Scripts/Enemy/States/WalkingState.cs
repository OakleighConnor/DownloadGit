using UnityEngine;
using Fusion.Addons.FSM;

namespace Enemy
{
    public class WalkingState : EnemyStateBehaviour
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
            //enemy.hr.SetHitboxActive(enemy.staggeredHitbox, false);
            enemy.activeSpeed = enemy.walkSpeed;
            Debug.Log("Walking");
        }

        protected override void OnFixedUpdate()
        {
            enemy.AllignZPos();

            if(enemy.CheckForEdge() || enemy.CheckForCollision())
            {
                enemy.Turn();
            }
        }
        
        protected override void OnExitState()
        {
            enemy.activeSpeed = 0;
        }

        protected override void OnEnterStateRender()
        {
            enemy.anim.Play("Walk");
        }

        protected override void OnRender()
        {
            
        }

        protected override void OnExitStateRender()
        {

        }
    }
}