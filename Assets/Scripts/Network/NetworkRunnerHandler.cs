using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Linq;
using Fusion.Addons.Physics;

public class NetworkRunnerHandler : MonoBehaviour
{
    public NetworkRunner networkRunnerPF;
    NetworkRunner networkRunner;

    

    public void Awake()
    {
        Debug.developerConsoleVisible = true;

        NetworkRunner networkRunnerInScene = FindAnyObjectByType<NetworkRunner>();

        // If we already have a Network Runner in the scene, don't create another one. Instead use an existing one.
        if(networkRunnerInScene != null)
        {
            networkRunner = networkRunnerInScene;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(networkRunner == null)
        {
            networkRunner = Instantiate(networkRunnerPF);
            networkRunner.name = "Network runner";

            if (SceneManager.GetActiveScene().name != "Menu")
            {
                bool publicity = true;
                var clientTask = InitializeNetworkRunner(networkRunner, GameMode.AutoHostOrClient, "TestSession", NetAddress.Any(),  SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex), publicity);
            }
            
            Debug.Log($"Server NetworkRunner");
        }
    
    }

    protected virtual Task InitializeNetworkRunner(NetworkRunner runner, GameMode gamemode, string sessionName , NetAddress address, SceneRef scene, bool publicity)
    {
        var sceneManager = runner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneManager>().FirstOrDefault();

        if (sceneManager == null)
        {
            sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        runner.ProvideInput = true;

        var runnerSimulatePhysics3D = runner.gameObject.AddComponent<RunnerSimulatePhysics3D>();
        runnerSimulatePhysics3D.ClientPhysicsSimulation = ClientPhysicsSimulation.SimulateAlways;

        

        return runner.StartGame(new StartGameArgs // Removed Initialized due to error. Possibly outdated in Fusion 2
        {
            GameMode = gamemode,
            Address = address,
            Scene = scene,
            SessionName = sessionName,
            CustomLobbyName = "OurLobbyID",
            SceneManager = sceneManager,
            SessionProperties = new Dictionary<string, SessionProperty>() 
            {
                { "Public", publicity},
                { "PlayerCount", 0},
                { "SessionName", PlayerPrefs.GetString("PlayerNickname")}
            }

        });
    }

    public void OnJoinLobby()
    {
        var clientTask = JoinLobby();
    }

    async Task JoinLobby()
    {
        Debug.Log("JoinLobby started");

        string lobbyID = "OurLobbyID";

        var result = await networkRunner.JoinSessionLobby(SessionLobby.Custom, lobbyID);

        if(!result.Ok)
        {
            Debug.LogError($"Unable to join lobby {lobbyID}");
        }
        else
        {
            Debug.Log("JoinLobby Ok");
        }
    }

    public void CreateGame(string sessionName, string sceneName)
    {
        Debug.Log($"Create session {sessionName} scene {sceneName} build Index {SceneUtility.GetBuildIndexByScenePath($"scenes/{sceneName}")}");

        bool publicity = true;

        var clientTask = InitializeNetworkRunner(networkRunner, GameMode.Host, sessionName, NetAddress.Any(), SceneRef.FromIndex(SceneUtility.GetBuildIndexByScenePath($"scenes/{sceneName}")), publicity);
    }

    public void JoinGame(string sessionName)
    {
        Debug.Log($"Join session {sessionName}");

        bool publicity = true;

        // Join existing game as a client
        var clientTask = InitializeNetworkRunner(networkRunner, GameMode.Client, sessionName, NetAddress.Any(), SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex), publicity);
    }
}
