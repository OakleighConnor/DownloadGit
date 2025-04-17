using UnityEngine;
using Fusion.Addons.FSM;

namespace Enemy
{
    public class EnemyStateBehaviour : StateBehaviour<EnemyStateBehaviour>
    {
        [Header("Components")]
        public EnemyScript enemy;

        // Called by the Player script when initializing the FSM
        public void Initialize(EnemyScript enemy)
        {
            this.enemy = enemy;
        }
    }

}
