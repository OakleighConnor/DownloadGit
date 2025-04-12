
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using Fusion.Addons.FSM;
using Fusion;
using UnityEditor.Experimental.GraphView;

namespace Player
{
    public class PlayerStateBehaviour  : StateBehaviour<PlayerStateBehaviour>
    {
        [Header("Components")]
        public PlayerScript player;

        [Header("Inputs")]
        public NetworkButtons buttons;
        public byte jumpInput;
        public Vector3 dir;

        // Called by the Player script when initializing the FSM
        public void Initialize(PlayerScript player)
        {
            this.player = player;
        }
    }

}