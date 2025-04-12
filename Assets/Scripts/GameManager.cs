using Fusion;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    // Prefabs
    public GameObject playerInputPF;

    // Local Play
    bool localPlay;

    // Scripts
    PlayerInputScript inputP1, inputP2;
    public Spawner networkRunnerScript;

    public override void Spawned()
    {
        Debug.Log("Spawned");
        //if(!HasInputAuthority) return;
        Debug.Log("HasInputAuthority");

        //networkRunnerScript = FindAnyObjectByType<Spawner>();

        if(!networkRunnerScript)
        {
            Debug.LogError("No NetworkRunner object has been found");
            return;
        }

        Debug.Log("NetworkRunner successfully assigned");

        localPlay = CheckForLocalPlay();

        // Instantiate player's objects
        networkRunnerScript.inputP1 = Instantiate(playerInputPF, transform, worldPositionStays: false).GetComponent<PlayerInputScript>();
        //networkRunnerScript.cameraP1 = inputP1.GetComponentInChildren<CameraScript>();

        if(!localPlay) return;
        // Instantiate player's objects
        networkRunnerScript.inputP2 = Instantiate(playerInputPF, transform, worldPositionStays: false).GetComponent<PlayerInputScript>();
        //networkRunnerScript.cameraP2 = inputP2.GetComponentInChildren<CameraScript>();
    }

    bool CheckForLocalPlay()
    {
        int value = PlayerPrefs.GetInt("LocalPlay");

        if (value == 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
