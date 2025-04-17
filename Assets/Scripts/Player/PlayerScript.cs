using Fusion;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Fusion.Addons.FSM;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Player
{
    [RequireComponent(typeof(StateMachineController))]
    public class PlayerScript : NetworkBehaviour, IDamageable, IStateMachineOwner
    {
        [Header("Local Multiplayer")] // These must be set in the prefabs
        public bool localPlayer;
        public UnityEvent<NetworkInputData> applyInput;

        [Header("Prefabs")]
        public GameObject playerInputPF;

        [Header("Inputs")]
        public byte jumpInput;
        public NetworkButtons buttons;
        public Vector2 dir;

        [Header("Player Movement")]
        public NetworkCharacterController cc;
        public int bounceHeight;

        [Header("Animation")]
        public Animator anim;
        public string currentAnim;
        public string lastAnim;
        public int staggerDuration;

        [Header("Collision + Hitboxes")]
        public LayerMask ground;
        public LayerMask headHitbox;
        public float rayOffsetX; // = 0f;
        public float rayOffsetY; // = -1.05f;
        public float checkLength; // = 0.4f; 
        public GameObject hitboxes;

        [Header("Camera")]
        public Transform cameraPos;
        
        [Header("States")]
        public IdleState _idleState;
        public MovementState _movementState;
        public JumpState _jumpState;
        public FallingState _fallingState;
        public StaggeredState _staggeredState;
        [Header("StateMachines")]
        private StateMachine<PlayerStateBehaviour> playerMachine;

        // WHEN CREATING NEW STATES AND MACHINES UPDATE THIS METHOD
        void IStateMachineOwner.CollectStateMachines(List<IStateMachine> stateMachines) // Creates State Machine, Initializes States & Assigns State Transitions 
        {
            // Creates new state machines
            playerMachine = new StateMachine<PlayerStateBehaviour>("Player Behaviour", _idleState, _movementState, _jumpState, _fallingState, _staggeredState);

            // Assign script reference in each of the states
            _idleState.Initialize(this);
            _movementState.Initialize(this);
            _jumpState.Initialize(this);
            _fallingState.Initialize(this);
            _staggeredState.Initialize(this);

            // Assign transitions between states

            // Idle Transitions
            _idleState.AddTransition(_movementState, CheckForMovement);
            _idleState.AddTransition(_jumpState, CheckForJump);
            _idleState.AddTransition(_fallingState, CheckForFall);

            // Movement Transitions
            _movementState.AddTransition(_idleState, CheckForIdle);
            _movementState.AddTransition(_jumpState, CheckForJump);
            _movementState.AddTransition(_fallingState, CheckForFall);

            // Jump Transitions
            _jumpState.AddTransition(_idleState, CheckForIdle);
            _jumpState.AddTransition(_movementState, CheckForMovement);
            _jumpState.AddTransition(_fallingState, CheckForFall);

            // Fall Transitions
            _fallingState.AddTransition(_idleState, CheckForIdle);
            _fallingState.AddTransition(_movementState, CheckForMovement);
            _fallingState.AddTransition(_jumpState, CheckForBounce);
            
            // Adds created state machines to state machines
            stateMachines.Add(playerMachine);
        }

        private void Awake()
        {
            cc = GetComponent<NetworkCharacterController>();
            anim = GetComponentInChildren<Animator>();
        }
        public override void Spawned() // Spawns player input component + canera and assigns input variables in the Spawner script
        {
            if(HasInputAuthority)
            {
                Spawner networkRunnerScript = FindAnyObjectByType<Spawner>();
                
                if(networkRunnerScript == null)
                {
                    Debug.LogError("No NetworkRunnerScript found. Did you try entering play mode from the Menu instead of the level?");
                    return;
                }

                PlayerInputScript playerInputScript = Instantiate(playerInputPF).GetComponent<PlayerInputScript>();
                playerInputScript.GetComponentInChildren<Camera>().enabled = true;
                playerInputScript.GetComponentInChildren<CameraScript>().playerCameraPos = cameraPos;

                if(!localPlayer)
                {
                    // Player 1
                    networkRunnerScript.inputP1 = playerInputScript;
                }
                else
                {
                    // Player 2
                    networkRunnerScript.inputP2 = playerInputScript;
                }
            }
            else
            {
                // Doesn't have input authority
            }
        }
        
        public override void FixedUpdateNetwork()
        {
            if(GetInput(out NetworkInputData data))
            {
                applyInput.Invoke(data);
            }
        }
        bool Grounded() // Performs a raycast to check for ground layer
        {
            // Raycast values
            Vector3 rayPos;
            rayPos = new Vector3(transform.position.x + rayOffsetX, transform.position.y + rayOffsetY, transform.position.z);

            // Debug
            Debug.Log("Ground Check");
            Debug.DrawRay(rayPos, Vector3.down * checkLength, Color.green);

            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(rayPos, Vector3.down, out hit, checkLength, ground))
            {
                Debug.Log("Grounded");
                return true;
            }
            else return false;
        }
        bool CheckForIdle() => dir.x == 0 && Grounded(); // Checks for no player movement input & ground
        bool CheckForMovement() => dir.x != 0 && Grounded(); // Checks for player movement input & ground
        bool CheckForJump() => buttons.IsSet(jumpInput) && Grounded(); // Checks for player jump inputs & ground
        bool CheckForFall() => cc.Velocity.y <= 0 && !Grounded(); // Checks for downwards velocity & no ground
        bool CheckForBounce() // Checks for hitboxes below the player & calls the interface IDamageable if the hitbox contains it
        {
            // Raycast values
            Vector3 rayPos;
            rayPos = new Vector3(transform.position.x, transform.position.y + rayOffsetY, transform.position.z);

            // Debugging
            Debug.DrawRay(rayPos, Vector3.down * checkLength, Color.red);

            // Raycast
            LagCompensatedHit hitInfo;
            if(Runner.LagCompensation.Raycast(rayPos, Vector3.down, checkLength, Object.InputAuthority, out hitInfo, headHitbox, HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority))
            {
                Debug.Log("Hit");

                // Checks for IDamageable
                IDamageable damageable = hitInfo.GameObject.GetComponentInParent<IDamageable>();
                if(damageable != null) 
                {
                    Debug.Log("IDamageable found. Calling Damage()");
                    damageable.Damage();
                }
                return true;
            }
            return false;
        }
        public void ApplyInputP1(NetworkInputData data) // Sets dir, buttons & jumpInput to Player 1 Inputs
        {
            dir = data.directionP1;
            buttons = data.buttonsP1;
            jumpInput = NetworkInputData.jumpP1;
        }
        public void ApplyInputP2(NetworkInputData data) // Sets dir, buttons & jumpInput to Player 2 Inputs
        {
            dir = data.directionP2;
            buttons = data.buttonsP2;
            jumpInput = NetworkInputData.jumpP2;
        }
        public void Move()
        {
            dir.Normalize();
            cc.Move(cc.maxSpeed * dir * 1000 * Runner.DeltaTime);
        }
        public void Damage()
        {
            Debug.Log("DAMAGED");
            playerMachine.ForceActivateState<StaggeredState>();
        }
    }

}


