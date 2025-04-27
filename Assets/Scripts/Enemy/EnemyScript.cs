using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;
using Fusion.Addons.FSM;
using System.Collections.Generic;

namespace Enemy
{
    [RequireComponent(typeof(StateMachineController))]
    public class EnemyScript : NetworkBehaviour, IDamageable, IPickupable, IStateMachineOwner
    {
        public GameObject holdPos;

        [Header("EnemyStats")]
        public float walkSpeed;
        public float activeSpeed;

        public float startSpinSpeed;
        public float staggerDuration;

        public float slideSpeed;
        public float slideDuration;

        [Header("Raycast Values")]
        public float rayLength;
        public LayerMask ground;
        public LayerMask player;
        public LayerMask collideable;
        public Transform groundCheckPos, edgeCheckPos;

        [Header("Hitboxes")]
        public HitboxRoot hr;
        public Hitbox activeHitbox;
        public Hitbox staggeredHitbox;
        
        [Header("Debugs")]
        public bool grounded;
        public bool nearEdge;

        [Header("Components")]
        public NetworkRigidbody3D rb;
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
        void Awake()
        {
            rb = GetComponent<NetworkRigidbody3D>();
            anim = GetComponentInChildren<Animator>();

            hr = GetComponent<HitboxRoot>();
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
        public override void FixedUpdateNetwork() 
        {
            Move();
        }

        public override void Spawned()
        {
        }
        public void Move()
        {
            if(activeSpeed == 0) return;
            
            rb.Rigidbody.AddForce(transform.forward * activeSpeed * 5 * 1000 * Runner.DeltaTime, ForceMode.Force);
            rb.Rigidbody.linearVelocity = Vector3.ClampMagnitude(rb.Rigidbody.linearVelocity, activeSpeed);
        }
        public void Turn() // Turn the enemy around
        {
            rb.Rigidbody.linearVelocity = new Vector3(0,0,0);
            transform.Rotate(0,180,0);
        }
        public bool CheckForCollision() // Checks for walls or other enemies and turns around if collision occurs
        {
            Debug.DrawRay(groundCheckPos.position, transform.forward * rayLength, Color.green);

            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(groundCheckPos.position, transform.forward, out hit, rayLength, collideable))
            {
                return true;
            }
            else return false;
        }
        public void CheckForPlayerCollision() // Checks for player. If player collision then damage the player
        {
            Vector3[] directions = new Vector3[] { Vector3.left, Vector3.right };

            foreach(Vector3 direction in directions)
            {
                Debug.DrawRay(groundCheckPos.position, direction * rayLength, Color.blue);

                LagCompensatedHit hitInfo;

                if(Runner.LagCompensation.Raycast(groundCheckPos.position, direction, rayLength * 2, Object.InputAuthority, out hitInfo, player, HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority))
                {
                    IDamageable damageable = hitInfo.GameObject.GetComponentInParent<IDamageable>();

                    // If enemy collides with a player then damage the player
                    if (damageable != null)
                    {
                        damageable.Damage();
                        return;
                    }
                }
            }
        }
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
            Debug.DrawRay(rayPos, Vector3.down * rayLength, Color.green);

            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(rayPos, Vector3.down, out hit, rayLength, ground))
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
