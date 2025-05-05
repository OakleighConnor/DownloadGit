using Fusion;
using UnityEngine;
using System.Collections.Generic;
public class CollisionManager : NetworkBehaviour
{
    public bool collisions;

    [Header("LayerMasks")]
    [SerializeField] LayerMask collideableLayers;
    [SerializeField] LayerMask damageLayer;
    [SerializeField] LayerMask collectableLayer;

    [Header("Collision Values")]
    [SerializeField] Vector3 hitboxSize;
    [SerializeField] Vector3 hitboxOffset;

    // Update is called once per frame
    public override void FixedUpdateNetwork()
    {
        if(HasStateAuthority && collisions)
        {
            CheckForCollision();
        }
    }

    void OnDrawGizmos() // Draws the OverlapBox for debugging purposes
    {
        if(collisions)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(hitboxOffset, hitboxSize);
        }
    }

    public void CheckForCollision() // Creates an OverlapBox that checks for hitboxes the player can collide with (Lag Compensated Collision)
    {
        // Results container
        List<LagCompensatedHit> hits = new List<LagCompensatedHit>();

        Vector3 hitboxPosition = new Vector3(transform.position.x + hitboxOffset.x ,transform.position.y + hitboxOffset.y ,transform.position.z + hitboxOffset.z);

        // OverlapBox
        int hitCount = Runner.LagCompensation.OverlapBox(
            hitboxPosition, // Location of overlap box on player
            hitboxSize, // Size of the hitbox
            transform.rotation, // Orientation
            Object.InputAuthority,
            hits, // Where the LagCompensated hits are stored
            collideableLayers, // Layers that should be read when checking for collisions
            HitOptions.IncludePhysX
        );

        Debug.Log($"Hit count {hitCount}");

        // Performs checks for all objects collided with
        for (int i = 0; i < hitCount; i++) // Executes for every layer that was collided with
        {
            Debug.Log($"Hitbox Layer: {hits[i].GameObject.layer}");
            CheckForDamage(hits[i]);
            CheckForCollectable(hits[i]);
        }
    }

    void CheckForDamage(LagCompensatedHit hit) // Checks if the collided object should damage the player. If so damage player
    {
        if (hit.GameObject.layer != Mathf.Log(damageLayer.value,2)) return; // hit.GameObject.layer references the layer's number rather than the LayerMask

        Debug.Log("Damage layer discovered");
        
        IDamageable damageable = GetComponent<IDamageable>(); // Gets IDamageable from the player to damage themselves

        if(damageable == null)
        {
            Debug.LogError("Player does not contain the interface IDamageable");
            return;
        }

        damageable.Damage();
    }

    void CheckForCollectable(LagCompensatedHit hit) // Checks if collided object is a collectable. If so collects it
    {
        if (hit.GameObject.layer != Mathf.Log(collectableLayer.value,2)) return; // hit.GameObject.layer references the layer's number rather than the LayerMask

        Debug.Log("Collectable layer discovered");

        ICollectable collectable = hit.GameObject.GetComponentInParent<ICollectable>(); // Gets ICollectable from the collectable the player collided with
        
        if(collectable == null)
        {
            Debug.LogError($"Object {hit.Hitbox.transform.root.gameObject} does not contain the interface ICollectable.");
            return;
        }

        collectable.Collect(GetComponent<PlayerScoreManager>());
        Runner.Despawn(hit.GameObject.GetComponent<NetworkObject>());
    }
}
