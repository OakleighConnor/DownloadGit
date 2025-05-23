using UnityEngine;
using Fusion;
public class CollectableScript : NetworkBehaviour, ICollectable
{
    public GameObject spawnLocation;
    public void Collect(PlayerScoreManager psm)
    {
        Debug.Log($"Collected {gameObject}");
        //psm.IncreaseScore();
        spawnLocation.SetActive(true); // Allows collectables to be spawned here in future
        Runner.Despawn(Object);
    }
}
