using Fusion;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Fusion.Addons.FSM;
using System.Collections.Generic;
using Unity.VisualScripting;
using Fusion.LagCompensation;

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
        byte jumpInput;
        byte actionInput;
        public NetworkButtons jumpButtons;
        public NetworkButtons actionButtons;
        public Vector2 dir;

        [Header("Player Movement")]
        public NetworkCharacterController cc;
        public int bounceHeight;
        [Networked] public bool moving { get; set; }

        [Header("Animation")]
        public Animator anim;

        [Header("Raycasts")]
        public LayerMask ground;
        public LayerMask headLayer;
        public LayerMask pickUpLayerMask;
        public float rayOffsetX; // = 0f;
        public float rayOffsetY; // = -1.05f;
        public float checkLength; // = 0.4f;

        [Header("Attack")]
        public Transform attackPos;
        public GameObject heldObjectPos;
        public GameObject heldObject;

        [Header("Hitboxes")]
        Hitbox[] hitboxes;
        HitboxRoot hr;

        [Header("Camera")]
        public Transform cameraPos;
        
        [Header("States")]
        [HideInInspector] public IdleState _idleState;
        [HideInInspector] public MovementState _movementState;
        [HideInInspector] public JumpState _jumpState;
        [HideInInspector] public FallingState _fallingState;
        [HideInInspector] public StaggeredState _staggeredState;
        [HideInInspector] public NeutralState _neutralState;
        [HideInInspector] public HoldingState _holdingState;

        [Header("StateMachines")]
        StateMachine<PlayerStateBehaviour> movementMachine;
        StateMachine<PlayerStateBehaviour> attackMachine;
        

        void IStateMachineOwner.CollectStateMachines(List<IStateMachine> stateMachines) // Creates State Machine, Initializes States & Assigns State Transitions. Update when implementing new states
        {
            // Movement FSM
            _idleState = GetComponentInChildren<IdleState>();
            _movementState = GetComponentInChildren<MovementState>();
            _jumpState = GetComponentInChildren<JumpState>();
            _fallingState = GetComponentInChildren<FallingState>();
            _staggeredState = GetComponentInChildren<StaggeredState>();

            // Attack FSM
            _neutralState = GetComponentInChildren<NeutralState>();
            _holdingState = GetComponentInChildren<HoldingState>();
            
            // Creates new state machines
            movementMachine = new StateMachine<PlayerStateBehaviour>("Movement Behaviour", _idleState, _movementState, _jumpState, _fallingState, _staggeredState);
            attackMachine = new StateMachine<PlayerStateBehaviour>("Attack Behaviour", _neutralState, _holdingState);

            // Assign script reference in each of the states
            _idleState.Initialize(this);
            _movementState.Initialize(this);
            _jumpState.Initialize(this);
            _fallingState.Initialize(this);
            _staggeredState.Initialize(this);
            _neutralState.Initialize(this);
            _holdingState.Initialize(this);

            // Assign transitions between states

            // Movement FSM
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
            if(Object.HasStateAuthority) _fallingState.AddTransition(_jumpState, CheckForBounce);

            // Attacking FSM
            // Neutral Transitions
            _neutralState.AddTransition(_holdingState, CheckForPickup);

            // Holding Transitions

            
            // Adds created state machines to state machines
            stateMachines.Add(movementMachine);
            stateMachines.Add(attackMachine);
        }
        private void Awake()
        {
            // Character Controller
            cc = GetComponent<NetworkCharacterController>();

            // Animation
            anim = GetComponentInChildren<Animator>();
            
            // Photon Fusion Hitboxes
            hitboxes = GetComponentsInChildren<Hitbox>();
            hr = GetComponent<HitboxRoot>();
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
                
            }
        }
        public override void FixedUpdateNetwork()
        {
            if(GetInput(out NetworkInputData data))
            {
                applyInput.Invoke(data);
            }

            Move();
            Debug.Log($"Action Input Pressed: {actionButtons.IsSet(actionInput)}");
        }
        bool Grounded() // Performs a raycast to check for ground layer
        {
            // Raycast values
            Vector3 rayPos;
            rayPos = new Vector3(transform.position.x + rayOffsetX, transform.position.y + rayOffsetY, transform.position.z);

            // Debug
            Debug.DrawRay(rayPos, Vector3.down * checkLength, Color.green);

            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(rayPos, Vector3.down, out hit, checkLength, ground))
            {
                return true;
            }
            else return false;
        }
        bool CheckForIdle() => dir.x == 0 && Grounded(); // Checks for no player movement input & ground
        bool CheckForMovement() => dir.x != 0 && Grounded(); // Checks for player movement input & ground
        bool CheckForJump() => jumpButtons.IsSet(jumpInput) && Grounded(); // Checks for player jump inputs & ground
        bool CheckForPickup() => actionButtons.IsSet(actionInput); // Checks if the player is using the action input (checks in state if a holdable object is close enough to the player)
        public bool CheckForThrow() => !actionButtons.IsSet(actionInput);
        public bool CheckForPickupTarget()
        {
            // Debugging
            Debug.DrawRay(attackPos.position, transform.forward * checkLength, Color.red);

            // Raycast
            LagCompensatedHit hitInfo;
            if(Runner.LagCompensation.Raycast(attackPos.position, transform.forward, checkLength, Object.InputAuthority, out hitInfo, pickUpLayerMask, HitOptions.IncludePhysX))
            {
                IPickupable pickupable = hitInfo.Hitbox.GetComponentInParent<IPickupable>();
                if(pickupable != null)
                {
                    Debug.Log("Can pick up");
                    pickupable.PickUp(heldObjectPos);
                    heldObject = hitInfo.Hitbox.transform.root.gameObject;
                    Debug.Log(heldObject);
                    return true;
                }
            }
            return false;
        }
        bool CheckForFall() => cc.Velocity.y <= 0 && !Grounded(); // Checks for downwards velocity & no ground
        bool CheckForBounce() // Performs lag compensated raycast checking for hitboxes. Must only be executed on state authority
        {
            // Raycast values
            Vector3 rayPos;
            rayPos = new Vector3(transform.position.x, transform.position.y + rayOffsetY, transform.position.z);

            // Debugging
            Debug.DrawRay(rayPos, Vector3.down * checkLength, Color.red);

            // Raycast
            LagCompensatedHit hitInfo;
            if(Runner.LagCompensation.Raycast(rayPos, Vector3.down, checkLength, Object.InputAuthority, out hitInfo, headLayer, HitOptions.IncludePhysX))
            {
                Debug.Log("Online Hit");
                InflictDamage(hitInfo.Hitbox.GetComponentInParent<IDamageable>());
                return true;
            }
            return false;
        }
        void InflictDamage(IDamageable damageable) // Calls Damage() in opponents that have been damaged
        {
            if(damageable == null) return;
            Debug.Log("IDamageable found. Calling Damage()");
            damageable.Damage();
        }
        public void ApplyInputP1(NetworkInputData data) // Sets dir, buttons & jumpInput to Player 1 Inputs
        {
            dir = data.directionP1;
            jumpButtons = data.jumpButtonsP1;
            actionButtons = data.actionButtonsP1;
            jumpInput = NetworkInputData.jumpP1;
            actionInput = NetworkInputData.actionP1;
        }
        public void ApplyInputP2(NetworkInputData data) // Sets dir, buttons & jumpInput to Player 2 Inputs
        {
            dir = data.directionP2;
            jumpButtons = data.jumpButtonsP2;
            actionButtons = data.actionButtonsP2;
            jumpInput = NetworkInputData.jumpP2;
            actionInput = NetworkInputData.actionP2;
        }
        public void Move() // cc.Move must always be called if gravity is to impact the player
        {
            float speed;
            if(moving)
            {
                speed = cc.maxSpeed;
            }
            else
            {
                speed = 0;
            }
            
            dir.Normalize();
            cc.Move(speed * dir * 1000 * Runner.DeltaTime);
        }
        public void Damage() // IDamageable Interface
        {
            Debug.Log("DAMAGED");
            movementMachine.ForceActivateState<StaggeredState>();
        }
        public void ToggleHitboxes(bool state) // Changes the state of the hixboxes to whatever is passed through the method
        {
            foreach(Hitbox hitbox in hitboxes)
            {
                hr.SetHitboxActive(hitbox, state);
            }
        }
    }

}


