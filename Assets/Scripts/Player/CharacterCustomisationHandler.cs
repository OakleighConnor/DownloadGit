using Fusion;
using NUnit.Framework;
using NUnit.Framework.Internal;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterCustomisationHandler : NetworkBehaviour
{
    [Networked]
    public NetworkBool isReady { get; set; }
    ChangeDetector cd;

    [Header("UI")]
    public TMP_Text readyText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        readyText.text = "";
    }
    public override void Spawned()
    {
        if(SceneManager.GetActiveScene().name == "Lobby")
        {
            if (HasInputAuthority)
            {
                FindAnyObjectByType<LobbyUIHandler>().cch = this;
            }

            Debug.Log("Lobby scene");
            cd = GetChangeDetector(ChangeDetector.Source.SimulationState);
            UpdateReadyUI(false);
            readyText.transform.rotation = Quaternion.Euler(0,180,0);
        }
        else
        {
            readyText.text = string.Empty;
        }
    }
    void OnEnable() // Subscribes the OnSceneLoaded method to the sceneLoaded event
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode) // Calls the Spawned method in suitable scenes
    {
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            if (Object.HasStateAuthority && Object.HasInputAuthority)
            {
                Spawned();
            }
        }
    }
    public override void Render()
    {
        ChangeDetection();
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

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_SetReady(NetworkBool isReady, RpcInfo info = default) // Changes the NetworkBool isReady for all clients triggering the ChangeDetector
    {
        this.isReady = isReady;
    }

}
