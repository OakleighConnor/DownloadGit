
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using Fusion.Addons.FSM;
using Fusion;

namespace Player
{
    public class PlayerStateBehaviour  : StateBehaviour<PlayerStateBehaviour>
    {
        protected PlayerScript player;
        protected NetworkButtons buttons;
        protected byte jumpInput;
        protected Vector3 dir;

        // Called by the Player script when initializing the FSM
        public void Initialize(PlayerScript player)
        {
            this.player = player;
        }
    }

}