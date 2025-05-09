using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.Diagnostics;
using Unity.VisualScripting;
using Player;
using System.Linq;
using ExitGames.Client.Photon.StructWrapping;

public class Spawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Prefabs")]
    public GameObject playerInputPF;
    public GameObject playerCamera;
    public GameObject gameManagerPF;
    [SerializeField] private NetworkPrefabRef player1PF, player2PF;

    [Header("ScriptableObjects")]
    public CodeManager codeManager;

    [Header("Player Objects")]
    NetworkObject playerObject1;
    NetworkObject playerObject2;

    [Header("LocalPlay")]
    public bool localPlay;
    int value;
    public PlayerInputScript inputP1, inputP2; // Make each client individually access this

    [Header("Scripts")]
    SessionListUIHandler sessionListUIHandler;

    void Awake()
    {
        value = PlayerPrefs.GetInt("LocalPlay");

        Debug.Log(value);

        if (value == 1)
        {
            localPlay = true;
        }
        else
        {
            localPlay = false;
        }
        
        // UI Elements
        sessionListUIHandler = FindAnyObjectByType<SessionListUIHandler>(FindObjectsInactive.Include);

        /*inputP1 = Instantiate(playerInputPF, transform, worldPositionStays: false).GetComponent<PlayerInputScript>();

        if (localPlay)
        {
            inputP2 = Instantiate(playerInputPF, transform, worldPositionStays: false).GetComponent<PlayerInputScript>();
        }

        cameraP1 = Instantiate(playerCamera).GetComponent<CameraScript>();
        if(localPlay) cameraP2 = Instantiate(playerCamera).GetComponent<CameraScript>();*/
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) 
    {
        if (runner.IsServer)
        {
            Debug.Log("OnPlayerJoined we are server. Spawning player");

            playerObject1 = SpawnPlayer(runner, player1PF, new Vector3(0, 10, 0), player);
            if (PlayerPrefs.GetInt("LocalPlay") == 1) playerObject2 = SpawnPlayer(runner, player2PF, new Vector3(10, 10, 0), player);

            UpdatePlayerCount(runner);
        }
        else
        {
            Debug.Log("OnPlayerJoined");
        }
    }
    NetworkObject SpawnPlayer(NetworkRunner runner, NetworkPrefabRef playerPF, Vector3 spawnLocation, PlayerRef player) // Spawns the player on the runner in a determined location depending on the scene
    {
        if(SceneManager.GetActiveScene().name == "Lobby")
        {
            PlayerScript[] players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);

            spawnLocation = new Vector3(players.Length * 3 - 4.5f, 0, 0);
        }
        return runner.Spawn(playerPF, spawnLocation, Quaternion.identity, player);
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) 
    {
        if(runner.IsServer)
        {
            UpdatePlayerCount(runner);
            runner.Despawn(playerObject1);
            if(playerObject2 != null) runner.Despawn(playerObject2);
        }
    }
    void UpdatePlayerCount(NetworkRunner runner)
    {
        PlayerScript[] players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);
        runner.SessionInfo.UpdateCustomProperties(new Dictionary<string, SessionProperty> {{ "PlayerCount", players.Length}});
    }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        if(inputP1 != null)
        {
            data.directionP1 += inputP1.movementInput;
            data.jumpButtonsP1.Set(NetworkInputData.jumpP1, inputP1.jumpInput);
            data.actionButtonsP1.Set(NetworkInputData.actionP1, inputP1.actionInput);
            inputP1.jumpInput = false;
        }

        if(inputP2 != null)
        {
            data.directionP2 += inputP2.movementInput;
            data.jumpButtonsP2.Set(NetworkInputData.jumpP2, inputP2.jumpInput);
            data.actionButtonsP2.Set(NetworkInputData.actionP2, inputP2.actionInput);
            inputP2.jumpInput = false;
        }


        input.Set(data);
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) 
    {
        // Updates the helper's sessionlist so that it can compare new codes to existing ones to ensure all codes are unique
        codeManager.sessionList.Clear();
        foreach(SessionInfo sessionInfo in sessionList)
        {
            codeManager.sessionList.Add(sessionInfo.Name);
        }

        // Only update the list of sessions when the session list UI handler is active
        if(sessionListUIHandler == null) return;

        List<SessionInfo> publicSessions = new List<SessionInfo>();

        foreach (SessionInfo sessionInfo in sessionList)
        {
            sessionInfo.Properties.TryGetValue("Public", out var publicSession);

            Debug.Log($"Found Session: {sessionInfo.Name} PlayerCount: {sessionInfo.PlayerCount} Public: {publicSession}");

            if (publicSession && sessionInfo.IsOpen)
            {
                if(publicSessions.Count == 0)
                {
                    sessionListUIHandler.ClearList();
                }
                sessionListUIHandler.AddToList(sessionInfo);

                publicSessions.Add(sessionInfo);
            }
        }

        if (publicSessions.Count == 0)
        {
            Debug.Log("Joined Lobby no public sessions found");

            sessionListUIHandler.OnNoSessionFound();
        }
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) 
    {
        Debug.Log("New Scene Loaded");
    }
    public void OnSceneLoadStart(NetworkRunner runner)
    {
        Debug.Log("Loading New Scene Start");
    }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){ }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){ }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data){ }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress){ }
}