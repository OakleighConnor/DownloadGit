using Fusion;
using Fusion.Addons.Physics;
using Fusion.LagCompensation;
using UnityEngine;
using Fusion.Addons.FSM;
using System.Collections.Generic;
using Player;
namespace Enemy
{
    [RequireComponent(typeof(StateMachineController))]
    public class EnemyScript : NetworkBehaviour, IDamageable, IStateMachineOwner
    {
        [Header("EnemyStats")]
        public float walkSpeed;
        public float activeSpeed;

        [Header("Raycast Values")]
        public LayerMask ground;
        public LayerMask collidable;
        public LayerMask player;
        public Transform edgeCheckPos;
        
        [Header("Debugs")]
        public bool grounded, nearEdge, collided;

        [Header("Components")]
        public NetworkRigidbody3D rb;

        [Header("States")]
        public WalkingState _walkingState;
        public TurningState _turningState;
        public FallingState _fallingState;
        public StaggeredState _staggeredState;
        [Header("StateMachines")]
        private StateMachine<EnemyStateBehaviour> enemyMachine;


        void Awake()
        {
            rb = GetComponent<NetworkRigidbody3D>();
        }

        void IStateMachineOwner.CollectStateMachines(List<IStateMachine> stateMachines) // Creates State Machine, Initializes States & Assigns State Transitions
        {
            enemyMachine = new StateMachine<EnemyStateBehaviour>("Enemy Behaviour", _walkingState, _turningState, _fallingState, _staggeredState);
            
            // Assign script reference in each of the states
            _walkingState.Initialize(this);
            _turningState.Initialize(this);
            _fallingState.Initialize(this);
            _staggeredState.Initialize(this);

    
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
            /*grounded = Grounded();

            if(Grounded())
            {
                if(CheckForEdge() || CheckForCollision())
                {
                    Debug.Log("Near Edge");
                    Turn();
                }
                else 
                {
                    Move();
                }
            }*/

            Move();
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

        /*bool Grounded(Vector3 rayPos) // Performs a raycast to check for ground layer
        {
            Debug.Log("Ground Check");

            // Debug
            Debug.DrawRay(rayPos, Vector3.down * 0.5f, Color.green);

            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(rayPos, Vector3.down, out hit, 0.7f, ground))
            {
                Debug.Log("Grounded");
                return true;
            }
            else return false;
        }*/

        public bool CheckForCollision() // Checks for walls or other enemies and turns around if collision occurs
        {
            Debug.DrawRay(transform.position, transform.forward * 1, Color.green);

            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position, transform.forward, out hit, 1, collidable))
            {
                return true;
            }
            else return false;
        }

        public void CheckForPlayerCollision() // Checks for player. If player collision then damage the player
        {
            Vector3[] directions = new Vector3[] { Vector3.left, Vector3.right };

            float checkLength = .25f;

            foreach(Vector3 direction in directions)
            {
                LagCompensatedHit hitInfo;

                if(Runner.LagCompensation.Raycast(transform.position, direction, checkLength * 2, Object.InputAuthority, out hitInfo, player, HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority))
                {
                    IDamageable damageable = hitInfo.GameObject.GetComponentInParent<IDamageable>();

                    // If enemy collides with a player then damage the player
                    if (damageable != null)
                    {
                        Debug.Log("Hit Player");
                        damageable.Damage();
                        return;
                    }
                }
            }
        }

        public void Damage()
        {
            Debug.Log("DAMAGED ENEMY");
            enemyMachine.ForceActivateState<StaggeredState>();
        }

        public bool CheckForEdge() => !RaycastDown(edgeCheckPos.position); // True if there is no ground in the position the enemy checks for the edge
        public bool CheckForLand() => RaycastDown(transform.position); // True if ground is beneath the enemy
        public bool CheckForFall() => !RaycastDown(transform.position);
        public bool RaycastDown(Vector3 rayPos) // Performs the Raycasts down
        {
            // Debug
            Debug.DrawRay(rayPos, Vector3.down * 0.5f, Color.green);

            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(rayPos, Vector3.down, out hit, 0.7f, ground))
            {
                Debug.Log("Grounded");
                return true;
            }
            else return false;
        }
    }

}
