using UnityEngine;
using Fusion;
using Fusion.Addons.FSM;
using Unity.VisualScripting.FullSerializer;

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
            enemy.anim.Play("Stagger");
            enemy.ToggleHitboxes(false);
        }

        protected override void OnFixedUpdate()
        {
            if (Machine.StateTime >= enemy.staggerDuration) // Once enemy has stopped spinning, end the stagger
            {
                Machine.TryDeactivateState(StateId);
            }
        }
        
        protected override void OnExitState()
        {
            enemy.ToggleHitboxes(true);
        }

        protected override void OnEnterStateRender()
        {
            enemy.anim.speed = enemy.startSpinSpeed;
        }

        protected override void OnRender()
        {
            if(Machine.StateTime <= 0) return;


            
            float v = enemy.startSpinSpeed * (Mathf.Round(Machine.StateTime * 100) / 100 / enemy.staggerDuration);
            Debug.Log(v);

            enemy.anim.speed = enemy.startSpinSpeed - v;
        }

        protected override void OnExitStateRender()
        {
            enemy.anim.speed = 1;
        }
    }
}