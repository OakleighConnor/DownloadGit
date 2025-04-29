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
            enemy.activeSpeed = enemy.slideSpeed;

            enemy.hr.SetHitboxActive(enemy.activeHitbox, true);
            enemy.hr.SetHitboxActive(enemy.staggeredHitbox, false);
        }

        protected override void OnFixedUpdate()
        {
            if (Machine.StateTime > enemy.slideDuration) Machine.TryDeactivateState(StateId); // Deactivate if slide ended
            if (Machine.StateTime > 0.5) enemy.CheckForPlayerCollision(); // Delay checking to damage players to ensure player who threw enemy doesn't take damage

            enemy.Slide(Machine.StateTime); // Perfom the slide

            if (enemy.CheckForCollision()) enemy.Turn(); // If the enemy collides with a wall then bounce
        }
        
        protected override void OnExitState()
        {
        }

        protected override void OnEnterStateRender()
        {
            enemy.anim.Play("Stagger");
            enemy.anim.speed = enemy.startSpinSpeed;
        }

        protected override void OnRender()
        {
            enemy.DecreaseAnimationSpeed(Mathf.Round(Machine.StateTime * 100) / 100, enemy.slideDuration);
        }

        protected override void OnExitStateRender()
        {
            enemy.anim.speed = 1;
        }
    }
}