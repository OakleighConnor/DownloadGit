using Fusion;
using UnityEngine;
using UnityEngine.Events;
using Fusion.Addons.FSM;
using System.Collections.Generic;
using Fusion.Addons.SimpleKCC;
using UnityEngine.SceneManagement;

namespace Player
{
    [RequireComponent(typeof(StateMachineController))]
    public class PlayerScript : NetworkBehaviour, IDamageable, IStateMachineOwner
    {
        public bool inLobby;
        public bool gameOver;

        [Header("Invinsibility")]
        [Networked] public TickTimer invincibility { get; set; }
        public static byte invincibilityDuration = 2;

        public AudioManager am;

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
        int faceDir;
        [Networked] NetworkButtons previousButtons {get; set;}

        [Header("Player Movement")]
        SimpleKCC kcc;
        public int speed;
        int currentSpeed;
        public int jumpImpulse;
        public int bounceHeight;
        public int jump;
        [HideInInspector] public bool moving = true;

        [Header("Animation")]
        public Animator anim;
        public GameObject body;

        [Header("Raycasts")]
        public bool grounded;
        public LayerMask ground;
        public LayerMask headLayer;
        public LayerMask pickUpLayerMask;
        public LayerMask hitboxCollisionLayerMask;
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
        public CollisionManager cm;

        [Header("Camera")]
        public Transform cameraPos;
        
        [Header("States")]
        [HideInInspector] public MenuState _menuState;
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
            _menuState = GetComponentInChildren<MenuState>();

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
            movementMachine = new StateMachine<PlayerStateBehaviour>("Movement Behaviour", _menuState, _idleState, _movementState, _jumpState, _fallingState, _staggeredState);
            attackMachine = new StateMachine<PlayerStateBehaviour>("Attack Behaviour", _neutralState, _holdingState);

            // Assign script reference in each of the states
            _menuState.Initialize(this);
            _idleState.Initialize(this);
            _movementState.Initialize(this);
            _jumpState.Initialize(this);
            _fallingState.Initialize(this);
            _staggeredState.Initialize(this);
            _neutralState.Initialize(this);
            _holdingState.Initialize(this);

            // Assign transitions between states

            // Movement FSM
            // Menu Transitions
            _menuState.AddTransition(_idleState, CheckForGame);

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
            if(Object.HasStateAuthority) _fallingState.AddTransition(_jumpState, CheckForBounce);

            // Attacking FSM
            // Neutral Transitions
            if(Object.HasStateAuthority) _neutralState.AddTransition(_holdingState, CheckForPickup);

            // Holding Transition

            
            // Adds created state machines to state machines
            stateMachines.Add(movementMachine);
            stateMachines.Add(attackMachine);
            
        }

        public void StartIFrameTimer() => invincibility = TickTimer.CreateFromSeconds(Runner, 5.0f);
        public bool IFramesEnded() => invincibility.Expired(Runner);
        private void Awake()
        {
            // Character Controller
            kcc = GetComponent<SimpleKCC>();

            // Animation
            anim = GetComponentInChildren<Animator>();

            // Photon Fusion Hitboxes
            hitboxes = GetComponentsInChildren<Hitbox>();
            hr = GetComponent<HitboxRoot>();

            // CollisionManager
            cm = GetComponent<CollisionManager>();

            // AudioManager
            am = FindAnyObjectByType<AudioManager>();

            moving = true;
        }
        public override void Spawned() // Spawns player input component + canera and assigns input variables in the Spawner script
        {
            invincibility = TickTimer.CreateFromSeconds(Runner, .1f);

            transform.rotation = Quaternion.Euler(transform.rotation.x, 90, transform.rotation.z);
            kcc.SetGravity(Physics.gravity.y * 7.5f);

            inLobby = SceneManager.GetActiveScene().name == "Lobby";

            if(HasInputAuthority && !inLobby)
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
        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"New Scene Loaded {scene}");

            if(SceneManager.GetActiveScene().name != "Lobby" && Object != null)
            {
                if(Object.HasStateAuthority && Object.HasInputAuthority)
                {
                    Spawned();
                }
            }
        }
        public override void FixedUpdateNetwork()
        {
            if (gameOver) return;
            
            if (GetInput(out NetworkInputData data))
            {
                applyInput.Invoke(data);
            }

            Move();

            if(SceneManager.GetActiveScene().name == "Lobby" || SceneManager.GetActiveScene().name == "Menu")
            {
                movementMachine.ForceActivateState<MenuState>();
            }
        }
        public override void Render()
        {
            RotatePlayer();
        }
        void RotatePlayer() // Rotates the player to face the last input direction
        {
            float rotation;
            if(faceDir == 0)
            {
                rotation = 180;
            }
            else
            {
                rotation = 90 * faceDir;
            }

            transform.rotation = Quaternion.Euler(transform.rotation.x, rotation, transform.rotation.z);
        }
        public void UpdatePlayerDirection() // Updates the direction the player should face
        {
            if(dir.x != 0) // Only updates direction if direction is being input
            {
                faceDir = (int)dir.x;
            }
        }
        public void Move() // Moves the player using the KCC
        {
            currentSpeed = speed;

            if(movementMachine.ActiveState == _staggeredState || movementMachine.ActiveState == _staggeredState)
            {
                currentSpeed = 0;
            }

            jump = 0;

            if(jumpButtons.WasPressed(previousButtons, jumpInput) && kcc.IsGrounded && movementMachine.ActiveState != _staggeredState)
            {
                movementMachine.TryActivateState<JumpState>();
            }

            kcc.Move(dir.normalized * currentSpeed, jump);

            previousButtons = jumpButtons;
        }
        public void Jump() // Resets the velocity of the player's jump
        {
            jump = jumpImpulse;
        }
        bool CheckForGame() => SceneManager.GetActiveScene().name != "Lobby" && SceneManager.GetActiveScene().name != "Menu"; // Checks for scene that isn't used for menus
        bool CheckForIdle() => dir.x == 0 && kcc.IsGrounded; // Checks for no player movement input & ground
        bool CheckForMovement() => dir.x != 0 && kcc.IsGrounded; // Checks for player movement input & ground
        //bool CheckForJump() => jumpButtons.IsSet(jumpInput) && kcc.IsGrounded; // Checks for player jump inputs & ground
        bool CheckForPickup() => actionButtons.IsSet(actionInput) && Object.HasStateAuthority; // Checks if the player is using the action input (checks in state if a holdable object is close enough to the player)
        public bool CheckForThrow() => !actionButtons.IsSet(actionInput) && Object.HasStateAuthority; // Checks for state authority and if the player lets go of the action button
        public bool CheckForPickupTarget() // Perform raycast checking if theres an object that can be picked up
        {
            Debug.DrawRay(attackPos.position, body.transform.forward * checkLength, Color.red);

            // Raycast
            LagCompensatedHit hitInfo;
            if (Runner.LagCompensation.Raycast(attackPos.position, body.transform.forward, checkLength, Object.InputAuthority, out hitInfo, pickUpLayerMask, HitOptions.IncludePhysX))
            {
                Debug.Log("Pickup layer");
                IThrowable pickupable = hitInfo.Hitbox.GetComponentInParent<IThrowable>();
                if (pickupable != null)
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
        public void ThrowObject() // Instantiates the object the player was holding and calls the Throw method in the IThrowable interface
        {
            if (heldObjectVisual == null || heldObjectPF == null) return;

            Destroy(heldObjectVisual);

            Debug.Log("Throw");

            heldObjectVisual = null;

            if (Object.HasStateAuthority)
            {
                Vector3 throwPos = new Vector3(holdPos.position.x, holdPos.position.y, 0);
                Quaternion rotation = Quaternion.Euler(transform.rotation.x, 90 * faceDir, transform.rotation.z);

                IThrowable throwable = Runner.Spawn(heldObjectPF, throwPos, rotation, Object.InputAuthority).GetComponentInParent<IThrowable>();
                Debug.Log($"Throwable : {throwable}");
                throwable.Throw();
            }
        }
        bool CheckForFall() => kcc.RealVelocity.y <= -0.1f; // If momentum down
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
            if(Runner.LagCompensation.Raycast(rightRayPos, Vector3.down, checkLength, Object.InputAuthority, out hitInfo, headLayer, HitOptions.IncludePhysX | HitOptions.SubtickAccuracy) || Runner.LagCompensation.Raycast(middRayPos, Vector3.down, checkLength, Object.InputAuthority, out hitInfo, headLayer, HitOptions.IncludePhysX | HitOptions.SubtickAccuracy) || Runner.LagCompensation.Raycast(leftRayPos, Vector3.down, checkLength, Object.InputAuthority, out hitInfo, headLayer, HitOptions.IncludePhysX | HitOptions.SubtickAccuracy))
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
            if (invincibility.Expired(Runner))
            {
                Debug.Log("DAMAGED");
                movementMachine.ForceActivateState<StaggeredState>();
                am.PlaySFX(am.playerDamage);
            }
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


