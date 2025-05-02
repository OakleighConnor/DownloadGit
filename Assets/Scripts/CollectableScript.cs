using UnityEngine;
using Fusion;
public class CollectableScript : NetworkBehaviour, ICollectable
{
    public void Collect()
    {
        Debug.Log($"Collected {gameObject}");
        Runner.Despawn(Object);
    }
}
