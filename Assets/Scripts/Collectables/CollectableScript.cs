using UnityEngine;
using Fusion;
public class CollectableScript : NetworkBehaviour, ICollectable
{
    public GameObject spawnLocation;
    AudioManager am;
    public void Awake()
    {
        am = FindAnyObjectByType<AudioManager>();   
    }
    public void Collect(PlayerScoreManager psm)
    {
        Debug.Log($"Collected {gameObject}");
        am.PlaySFX(am.collect);
        //psm.IncreaseScore();
        spawnLocation.SetActive(true); // Allows collectables to be spawned here in future
        Runner.Despawn(Object);
    }
}
