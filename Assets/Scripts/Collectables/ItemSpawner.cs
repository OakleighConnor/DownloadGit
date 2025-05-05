using Fusion;
using UnityEngine;
public class ItemSpawner : NetworkBehaviour
{
    public bool spawning;
    [Header("Floppy Disks")]
    public GameObject floppyDiskPF;
    public GameObject[] diskSpawnLocations;
    const byte diskTimerValue = 10;
    [Networked] private TickTimer timeUntilDiskSpawn { get; set; }
    void Awake()
    {
        diskSpawnLocations = GameObject.FindGameObjectsWithTag("DiskSpawnLocation");
    }
    public override void Spawned()
    {
        if(HasStateAuthority)
        {
            timeUntilDiskSpawn = TickTimer.CreateFromSeconds(Runner, diskTimerValue);
        }
    }
    public override void FixedUpdateNetwork()
    {
        if(!spawning || !HasStateAuthority) return;
        InstantiateFloppyDisks();
    }
    void InstantiateFloppyDisks() // When disk timer ends, finds random location and instantiates disk if the location is active. Disables location after instantiating disk
    {
        Debug.Log("Attempting to intantiate floppy disks");
        if(diskSpawnLocations.Length == 0)
        {
            Debug.LogError("No Disk Spawn Locations have been assigned. Have the Disk Spawn Locations recieved the 'DiskSpawnLocation' tag?");
            return;
        }

        if(timeUntilDiskSpawn.Expired(Runner))
        {
            Debug.Log("Instantiating Floppy Disk");
            GameObject randomSpawnLocation = diskSpawnLocations[Random.Range(1, diskSpawnLocations.Length)];
            if(randomSpawnLocation.activeSelf == false) return;

            NetworkObject floppyDisk = Runner.Spawn(floppyDiskPF, randomSpawnLocation.transform.position, Quaternion.identity);
            floppyDisk.GetComponent<CollectableScript>().spawnLocation = randomSpawnLocation;
            
            randomSpawnLocation.SetActive(false);
            timeUntilDiskSpawn = TickTimer.CreateFromSeconds(Runner, diskTimerValue);
        }
    }
}
