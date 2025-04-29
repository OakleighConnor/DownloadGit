using Fusion;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Fusion.Addons.FSM;
using System.Collections.Generic;
using Unity.VisualScripting;
using Fusion.LagCompensation;
using Enemy;
using Fusion.Addons.SimpleKCC;
using UnityEngine.TextCore;
using UnityEngine.Video;

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
        [Networked] NetworkButtons previousButtons {get; set;}

        [Header("Player Movement")]
        SimpleKCC kcc;
        public int speed;
        int currentSpeed;
        public int jumpImpulse;
        public int bounceHeight;
        int jump;
        [HideInInspector] public bool moving = true;

        [Header("Animation")]
        public Animator anim;
        public GameObject body;

        [Header("Raycasts")]
        public bool grounded;
        public LayerMask ground;
        public LayerMask headLayer;
        public LayerMask pickUpLayerMask;
        public float rayOffsetX; // = 0f;
        public float rayOffsetY; // = -1.05f;
        public float checkLength; // = 0.4f;

        [Header("Attack")]
        public Transform attackPos;
        public Transform holdPos;

        public GameObject heldObject;
        public GameObject heldObjectPF;
        public GameObject heldObjectVisual;

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
        //[HideInInspector] public NeutralState _neutralState;
        //[HideInInspector] public HoldingState _holdingState;

        [Header("StateMachines")]
        StateMachine<PlayerStateBehaviour> movementMachine;
        //StateMachine<PlayerStateBehaviour> attackMachine;
        

        void IStateMachineOwner.CollectStateMachines(List<IStateMachine> stateMachines) // Creates State Machine, Initializes States & Assigns State Transitions. Update when implementing new states
        {
            
            // Movement FSM
            _idleState = GetComponentInChildren<IdleState>();
            _movementState = GetComponentInChildren<MovementState>();
            _jumpState = GetComponentInChildren<JumpState>();
            _fallingState = GetComponentInChildren<FallingState>();
            _staggeredState = GetComponentInChildren<StaggeredState>();

            // Attack FSM
      /*      _neutralState = GetComponentInChildren<NeutralState>();
            _holdingState = GetComponentInChildren<HoldingState>();*/
            
            // Creates new state machines
            movementMachine = new StateMachine<PlayerStateBehaviour>("Movement Behaviour", _idleState, _movementState, _jumpState, _fallingState, _staggeredState);
            //attackMachine = new StateMachine<PlayerStateBehaviour>("Attack Behaviour", _neutralState, _holdingState);

            // Assign script reference in each of the states
            _idleState.Initialize(this);
            _movementState.Initialize(this);
            _jumpState.Initialize(this);
            _fallingState.Initialize(this);
            _staggeredState.Initialize(this);
  /*          _neutralState.Initialize(this);
            _holdingState.Initialize(this);*/

            // Assign transitions between states

            // Movement FSM
            // Idle Transitions
            _idleState.AddTransition(_movementState, CheckForMovement);
            _idleState.AddTransition(_fallingState, CheckForFall);

            // Movement Transitions
            _movementState.AddTransition(_idleState, CheckForIdle);
            _movementState.AddTransition(_fallingState, CheckForFall);

            // Jump Transitions
            _jumpState.AddTransition(_idleState, CheckForIdle);
            _jumpState.AddTransition(_movementState, CheckForMovement);
            _jumpState.AddTransition(_fallingState, CheckForFall);

            // Fall Transitions
            _fallingState.AddTransition(_idleState, CheckForIdle);
            _fallingState.AddTransition(_movementState, CheckForMovement);
            _fallingState.AddTransition(_jumpState, CheckForBounce);

            // Attacking FSM
            // Neutral Transitions
            //_neutralState.AddTransition(_holdingState, CheckForPickup);

            // Holding Transition

            
            // Adds created state machines to state machines
            stateMachines.Add(movementMachine);
            //stateMachines.Add(attackMachine);
            
        }
        private void Awake()
        {
            // Character Controller
            kcc = GetComponent<SimpleKCC>();

            // Animation
            anim = GetComponentInChildren<Animator>();
            
            // Photon Fusion Hitboxes
            hitboxes = GetComponentsInChildren<Hitbox>();
            hr = GetComponent<HitboxRoot>();

            moving = true;
        }
        public override void Spawned() // Spawns player input component + canera and assigns input variables in the Spawner script
        {
            kcc.SetGravity(Physics.gravity.y * 7.5f);

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

            /*jump = 0;

            if(jumpButtons.WasPressed(previousButtons, jumpInput) && kcc.IsGrounded)
            {
                jump = jumpImpulse;
            }

            kcc.Move(dir.normalized * speed, jump);
            previousButtons = jumpButtons;*/

            //Move();
            //Jump();
            //GroundCheck();
        }

        public void RotatePlayer()
        {
            if(dir.x != 0) body.transform.rotation = Quaternion.Euler(transform.rotation.x, 90 * dir.x, transform.rotation.z);
        }
        
        public void Move()
        {
            currentSpeed = speed;

            if(!moving)
            {
                currentSpeed = 0;
            }

            jump = 0;

            if(jumpButtons.WasPressed(previousButtons, jumpInput) && kcc.IsGrounded)
            {
                jump = jumpImpulse;
                movementMachine.TryActivateState<JumpState>();
            }

            kcc.Move(dir.normalized * currentSpeed, jump);
            previousButtons = jumpButtons;
        }
        bool CheckForIdle() => dir.x == 0 && kcc.IsGrounded; // Checks for no player movement input & ground
        bool CheckForMovement() => dir.x != 0 && kcc.IsGrounded; // Checks for player movement input & ground
        //bool CheckForJump() => jumpButtons.IsSet(jumpInput) && kcc.IsGrounded; // Checks for player jump inputs & ground
        bool CheckForPickup() => actionButtons.IsSet(actionInput); // Checks if the player is using the action input (checks in state if a holdable object is close enough to the player)
        public bool CheckForThrow() => !actionButtons.IsSet(actionInput);
        public bool CheckForPickupTarget()
        {
            // Debugging
            Debug.DrawRay(attackPos.position, Vector3.right * dir.x * checkLength, Color.red);

            // Raycast
            LagCompensatedHit hitInfo;
            if(Runner.LagCompensation.Raycast(attackPos.position, Vector3.right, checkLength * dir.x, Object.InputAuthority, out hitInfo, pickUpLayerMask, HitOptions.IncludePhysX))
            {
                Debug.Log("Pickup layer");
                IPickupable pickupable = hitInfo.Hitbox.GetComponentInParent<IPickupable>();
                if(pickupable != null)
                {
                    Debug.Log("Can pick up");
                    HoldObject(hitInfo.Hitbox.transform.root.gameObject, pickupable.PickUp());
                    return true;
                }
            }
            return false;
        }
        void HoldObject(GameObject obj, GameObject objectVisual) // Places the visual the player will hold where they should hold it
        {
            if (heldObjectVisual != null) return;
            heldObjectVisual = objectVisual;
            heldObjectPF = Resources.Load(obj.tag, typeof(GameObject)) as GameObject;

            heldObjectVisual.transform.SetParent(holdPos);
            heldObjectVisual.transform.position = holdPos.position;
        }

        public void ThrowObject()
        {
            if (heldObjectVisual == null || heldObjectPF == null) return;
            Destroy(heldObjectVisual);

            Debug.Log("Throw");

            heldObjectVisual = null;

            if(Object.HasStateAuthority) Runner.Spawn(heldObjectPF, holdPos.position, Quaternion.LookRotation(transform.forward), Object.InputAuthority);
        }
        //bool CheckForFall() => kcc.Velocity.y <= -0.1f; // Checks for downwards velocity & no ground
        bool CheckForFall() => kcc.RealVelocity.y <= -0.1f;
        bool CheckForBounce() // Performs lag compensated raycast checking for hitboxes. Must only be executed on state authority
        {
            // Raycast values
            Vector3 rightRayPos;
            Vector3 middRayPos;
            Vector3 leftRayPos;
            rightRayPos = new Vector3(transform.position.x + rayOffsetX, transform.position.y + rayOffsetY, transform.position.z);
            middRayPos = new Vector3(transform.position.x, transform.position.y + rayOffsetY, transform.position.z);
            leftRayPos = new Vector3(transform.position.x - rayOffsetX, transform.position.y + rayOffsetY, transform.position.z);

            // Debugging
            Debug.DrawRay(rightRayPos, Vector3.down * checkLength, Color.red);
            Debug.DrawRay(middRayPos, Vector3.down * checkLength, Color.red);
            Debug.DrawRay(leftRayPos, Vector3.down * checkLength, Color.red);

            // Raycast
            LagCompensatedHit hitInfo;
            if(Runner.LagCompensation.Raycast(rightRayPos, Vector3.down, checkLength, Object.InputAuthority, out hitInfo, headLayer, HitOptions.IncludePhysX) || Runner.LagCompensation.Raycast(middRayPos, Vector3.down, checkLength, Object.InputAuthority, out hitInfo, headLayer, HitOptions.IncludePhysX) || Runner.LagCompensation.Raycast(leftRayPos, Vector3.down, checkLength, Object.InputAuthority, out hitInfo, headLayer, HitOptions.IncludePhysX))
            {
                Debug.Log("Online Hit");
                InflictDamage(hitInfo.Hitbox.GetComponentInParent<IDamageable>());
                jump = jumpImpulse;
                kcc.Move(dir.normalized * speed, jump);
                movementMachine.TryActivateState<JumpState>();
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


