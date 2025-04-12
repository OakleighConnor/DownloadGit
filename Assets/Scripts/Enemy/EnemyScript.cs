using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

public class EnemyScript : NetworkBehaviour
{
    [Header("EnemyStats")]
    public float walkSpeed;

    [Header("Raycast Values")]
    public float rayLength;
    public LayerMask ground;
    public LayerMask collidable;
    public Transform edgeCheckPos;
    
    [Header("Debugs")]
    public bool grounded, nearEdge, collided;

    // ADD TO THE Z FOR FORWARD

    [Header("Components")]
    public NetworkRigidbody3D rb;

    void Awake()
    {
        rb = GetComponent<NetworkRigidbody3D>();
    }

    public override void FixedUpdateNetwork()
    {
        if(Grounded(transform.position))
        {
            if(CheckForEdge() || CheckForCollision())
            {
                Debug.Log("Near Edge");
                Turn();
            }
            else 
            {
                rb.Rigidbody.AddForce(transform.forward * walkSpeed * 5 * 1000 * Runner.DeltaTime, ForceMode.Force);
                rb.Rigidbody.linearVelocity = Vector3.ClampMagnitude(rb.Rigidbody.linearVelocity, walkSpeed);
            }
        }
    }

    void Turn()
    {
        rb.Rigidbody.linearVelocity = new Vector3(0,0,0);
        transform.Rotate(0,180,0);
    }

    bool Grounded(Vector3 rayPos) // Performs a raycast to check for ground layer
    {
        Debug.Log("Ground Check");

        // Debug
        Debug.DrawRay(rayPos, Vector3.down * rayLength, Color.green);

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(rayPos, Vector3.down, out hit, rayLength, ground))
        {
            Debug.Log("Grounded");
            return true;
        }
        else return false;
    }

    bool CheckForCollision()
    {
        Debug.DrawRay(transform.position, transform.forward * rayLength, Color.green);

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.forward, out hit, rayLength, collidable))
        {
            Debug.Log("Collision");
            return true;
        }
        else return false;
    }

    bool CheckForEdge()
    {
        return !Grounded(edgeCheckPos.position);
    }
}
