using Fusion;
using UnityEngine;
using Fusion.Addons.FSM;
using System.Collections.Generic;
using Fusion.Addons.SimpleKCC;
using Fusion.LagCompensation;
using UnityEditor;
using JetBrains.Annotations;
using Unity.VisualScripting;

namespace Enemy
{
    [RequireComponent(typeof(StateMachineController))]
    public class EnemyScript : NetworkBehaviour, IDamageable, IThrowable, IStateMachineOwner
    {
        public GameObject holdPos;

        [Header("EnemyStats")]
        public int walkSpeed;
        public int slideSpeed;
        public int activeSpeed;
        public float activeSlideSpeed;

        public float startSpinSpeed;
        public float staggerDuration;

        public float slideDuration;

        [Header("Collision")]
        public float groundRayLength;
        public float collisionRayLength;
        public LayerMask ground;
        public LayerMask player;
        public LayerMask collideable;
        public Transform groundCheckPos, edgeCheckPos;
        public Vector3 overlapBoxSize = new Vector3(0.4f, 0.4f, 0.4f);

        [Header("Gravity")]
        public float defaultGravity;
        public float slideGravity;

        [Header("Hitboxes")]
        public HitboxRoot hr;
        public Hitbox activeHitbox;
        public Hitbox staggeredHitbox;
        
        [Header("Debugs")]
        public bool grounded;
        public bool nearEdge;

        [Header("Components")]
        SimpleKCC kcc;
        public Animator anim;
        public GameObject body;

        [Header("States")]
        public WalkingState _walkingState;
        public FallingState _fallingState;
        public StaggeredState _staggeredState;
        public HeldState _heldState;
        public SlidingState _slidingState;

        [Header("StateMachines")]
        private StateMachine<EnemyStateBehaviour> enemyMachine;
        void Awake() // Assigns references to components
        {
            kcc = GetComponent<SimpleKCC>();
            hr = GetComponent<HitboxRoot>();
            anim = GetComponentInChildren<Animator>();
        }
        void IStateMachineOwner.CollectStateMachines(List<IStateMachine> stateMachines) // Creates State Machine, Initializes States & Assigns State Transitions
        {
            _walkingState = GetComponentInChildren<WalkingState>();
            _fallingState = GetComponentInChildren<FallingState>();
            _staggeredState = GetComponentInChildren<StaggeredState>();
            _heldState = GetComponentInChildren<HeldState>();
            _slidingState = GetComponentInChildren<SlidingState>();

            enemyMachine = new StateMachine<EnemyStateBehaviour>("Enemy Behaviour", _walkingState, _fallingState, _staggeredState, _heldState, _slidingState);
            
            // Assign script reference in each of the states
            _walkingState.Initialize(this);
            _fallingState.Initialize(this);
            _staggeredState.Initialize(this);
            _heldState.Initialize(this);
            _slidingState.Initialize(this);

    
            // Assign transitions between states

            // Walking Transitions
            _walkingState.AddTransition(_fallingState, CheckForFall);

            // Falling Transitions
            _fallingState.AddTransition(_walkingState, CheckForLand);
            
            // Adds created state machines to state machines
            stateMachines.Add(enemyMachine);
        }
        public override void Spawned()
        {
            hr.InitHitboxes();

            hr.SetHitboxActive(activeHitbox, true);
            hr.SetHitboxActive(staggeredHitbox, false);

            kcc.SetGravity(Physics.gravity.y * 4.0f);
        }
        public void Update()
        {
            if (enemyMachine == null) return;
            Debug.Log($"Enemy State: {enemyMachine.ActiveState}");
        }
        public void SetGravity(float gravity)
        {
            kcc.SetGravity(Physics.gravity.y * gravity);
        }
        public void AllignZPos() // Sets z pos to 0 to ensure enemy doesn't break
        {
            kcc.SetPosition(new Vector3(transform.position.x, transform.position.y, 0));
        }
        public override void FixedUpdateNetwork() 
        {
            Move();
        }
        public void Move() // Moves the enemy using the KCC 
        {
            kcc.Move(transform.forward * activeSpeed * 100 * Runner.DeltaTime, 0);
        }
        public void Slide(float stateTime) // Gradually decreases the speed of the enemy sliding over time. Applies movement to KCC 
        {
            if(Object.HasStateAuthority)
            {
                activeSlideSpeed = DecreaseValueOverTime(slideSpeed, stateTime, slideDuration);
                kcc.Move(kcc.transform.forward * activeSlideSpeed * 100 * Runner.DeltaTime, 0);
            }
        }
        public void DecreaseAnimationSpeed(float stateTime, float animationDuration) // Decreases animation speed over time 
        {
            anim.speed = DecreaseValueOverTime(startSpinSpeed, stateTime, animationDuration);
        }
        float DecreaseValueOverTime(float startingValue, float timePassed, float duration) // Used by sliding and staggered state. Gradually decreases value over time for the duration input 
        {
            float value;

            value = startingValue - (startingValue * timePassed / duration);

            return value;
        }
        public void Turn() // Turn the enemy around. Currently snaps to a 180 turn 
        {
            Debug.Log("Turning");
            kcc.AddLookRotation(new Vector3(0,180,0));
        }
        public bool CheckForCollision() // Checks for walls or other enemies and turns around if collision occurs 
        {
            Debug.DrawRay(edgeCheckPos.position, transform.forward * collisionRayLength, Color.blue);

            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(edgeCheckPos.position, transform.forward, out hit, collisionRayLength, collideable))
            {
                return true;
            }
            else return false;
        }
        void OnDrawGizmos() // Draws the OverlapBox for debugging purposes
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(new Vector3(0, 0.475f, 0), overlapBoxSize);
        }
        /*public void CheckForPlayerCollision() // Checks for player. If player collision then damage the player
        {
            // Results container
            List<LagCompensatedHit> hits = new List<LagCompensatedHit>();

            int hitCount = Runner.LagCompensation.OverlapBox(
                transform.position, // Location of overlap box
                overlapBoxSize, // Half extents
                transform.rotation, // Orientation
                Object.InputAuthority,
                hits, // Where the LagCompensated hits should be stored
                player, // The layer mask that should be read 
                HitOptions.IncludePhysX
            );

            // Calls Damage() for each player caught in the OverlapBox
            for (int i = 0; i < hitCount; i++)
            {
                Debug.Log($"Hit {i}");
                LagCompensatedHit hitInfo = hits[i];

                Debug.Log($"Hit Object {hitInfo.GameObject.transform.root.gameObject}");

                IDamageable damageable = hitInfo.GameObject.GetComponentInParent<IDamageable>();
                if(damageable != null)
                {
                    Debug.Log("Player with IDamageable found! Calling Damage()");
                    damageable.Damage();
                }
            }
        }*/
        public void Damage() // Triggers the StaggeredState of the enemy (IDamagable)
        {
            Debug.Log("DAMAGED ENEMY");
            enemyMachine.ForceActivateState<StaggeredState>();
        }
        public bool CheckForEdge() => !RaycastDown(edgeCheckPos.position); // True if there is no ground in the position the enemy checks for the edge
        public bool CheckForLand() => RaycastDown(groundCheckPos.position); // True if ground is beneath the enemy
        public bool CheckForFall() => !RaycastDown(groundCheckPos.position);
        public bool RaycastDown(Vector3 rayPos) // Performs the Raycasts down
        {
            // Debug
            Debug.DrawRay(rayPos, Vector3.down * groundRayLength, Color.green);

            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(rayPos, Vector3.down, out hit, groundRayLength, ground))
            {
                return true;
            }
            else return false;
        }
        /*public void PickUp(GameObject holdPos)
        {
            this.holdPos = holdPos;
            enemyMachine.ForceActivateState<HeldState>();
        }*/
        public GameObject PickUp() // Returns the graphic that the player will hold
        {
            Destroy(gameObject);
            return body;
        }

        public void Throw()
        {
            Debug.Log("Enemy thrown");
            enemyMachine.ForceActivateState<SlidingState>();
        }
    }

}
