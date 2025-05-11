using System;
using Fusion;
using NUnit.Framework;
using NUnit.Framework.Internal;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerReadyUpHandler : NetworkBehaviour
{
    [Networked]
    public NetworkBool isReady { get; set; }
    ChangeDetector cd;

    [Header("UI")]
    public TMP_Text readyText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        bool inLobby = SceneManager.GetActiveScene().name == "Lobby";

        readyText.enabled = inLobby;
    }
    public override void Spawned()
    {
        if(SceneManager.GetActiveScene().name == "Lobby")
        {
            if (HasInputAuthority)
            {
                LobbyUIHandler lobbyHandler = FindAnyObjectByType<LobbyUIHandler>();
                if (lobbyHandler != null) lobbyHandler.prhs.Add(this);
            }

            Debug.Log("Lobby scene");
            cd = GetChangeDetector(ChangeDetector.Source.SimulationState);
            UpdateReadyUI(isReady);
            readyText.transform.rotation = Quaternion.Euler(0,180,0);
        }
    }
    void OnEnable() // Subscribes the OnSceneLoaded method to the sceneLoaded event
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode) // Calls the Spawned method in suitable scenes
    {
        if (SceneManager.GetActiveScene().name == "Lobby" && Object != null)
        {
            if (Object.HasStateAuthority && Object.HasInputAuthority)
            {
                Spawned();
                ToggleReadyText(true);
            }
        }
    }
    private void OnSceneUnloaded(Scene scene) // Disables ready text
    {
        bool unloadingLobby = SceneManager.GetActiveScene().name == "Lobby";

        if(readyText != null) ToggleReadyText(false);
    }
    void ToggleReadyText(bool state) => readyText.enabled = state; // Toggles the state of the text that states if a player is ready
    public override void Render()
    {
        if (SceneManager.GetActiveScene().name == "Lobby") ChangeDetection();
    }
    void ChangeDetection() // Checks for changes in variables with the Networked attribute
    {
        foreach (var change in cd.DetectChanges(this, out var previousBuffer, out var currentBuffer))
        {
            switch (change)
            {
                case nameof(isReady):
                {
                    var reader = GetPropertyReader<NetworkBool>(nameof(isReady));
                    var (previous, current) = reader.Read(previousBuffer, currentBuffer);
                    OnReadyChanged(previous, current);
                    break;
                }
            }
        }
    }
    public void OnReady(bool state) // Called by LobbyUIHandler
    {
        if(Object.HasInputAuthority)
        {
            RPC_SetReady(state);
        }
    }
    void OnReadyChanged(bool previous, bool current) // Executes when change is detected in NetworkBool isReady 
    {
        if(current != previous)
        {
            UpdateReadyUI(current);
        }
    }
    void UpdateReadyUI(bool state) // Sets the UI of the player to display that they are ready
    {
        Debug.Log($"Ready: {state}");
        if(state)
        {
            readyText.text = "READY";
            readyText.color = Color.green;
        }
        else
        {
            readyText.text = "NOT READY";
            readyText.color = Color.red;
        }
    }
    bool AllPlayersReady() // Checks all of the CharacterCustomisationHandlers active in the scene. Returns false if any aren't ready
    {
        PlayerReadyUpHandler[] players = FindObjectsByType<PlayerReadyUpHandler>(FindObjectsSortMode.None);
        foreach(PlayerReadyUpHandler player in players)
        {
            if(!player.isReady) return false;
        }
        return true;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_SetReady(NetworkBool isReady, RpcInfo info = default) // Changes isReady bool for this game object. Manages the TickTimer within the LobbyUIHandler
    {
        this.isReady = isReady;

        LobbyUIHandler lobbyUIHandler = FindAnyObjectByType<LobbyUIHandler>();

        if (AllPlayersReady())
        {
            lobbyUIHandler.countdownTickTimer = TickTimer.CreateFromSeconds(Runner, lobbyUIHandler.countdownDuration);
        }
        else
        {
            lobbyUIHandler.countdownTickTimer = TickTimer.None;
            lobbyUIHandler.countdown = 0;
        }
    }
}
