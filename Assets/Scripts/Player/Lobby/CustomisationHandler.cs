using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
public class CustomisationHandler : NetworkBehaviour
{
    [SerializeField] Renderer playerBody;

    [Networked, OnChangedRender(nameof(OnAppearanceChanged))]
    Color color {get; set;}
    public void ChangeColor(Color color) // Changes the Networked color variable
    {
        if(HasInputAuthority) // Only update the color variable of the player who clicked the button
        {
            Debug.Log($"Updating player {gameObject}'s color to {color}");
            RPC_SetColor(color);
        }
    }
    void OnAppearanceChanged() // Updates how the player looks in the game
    {
        playerBody.material.color = color;
    }
    public override void Spawned()
    {
        if(SceneManager.GetActiveScene().name == "Lobby")
        {
            if (HasInputAuthority)
            {
                LobbyUIHandler lobbyHandler = FindAnyObjectByType<LobbyUIHandler>();
                if (lobbyHandler != null) lobbyHandler.customisations.Add(this);
            }

            Debug.Log("Lobby scene");
        }

        OnAppearanceChanged();
    }
    void OnEnable() // Subscribes the OnSceneLoaded method to the sceneLoaded event
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode) // Calls the Spawned method in suitable scenes
    {
        if (Object != null)
        {
            if (Object.HasStateAuthority && Object.HasInputAuthority)
            {
                Spawned();
            }
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    void RPC_SetColor(Color color, RpcInfo info = default) // Updates Networked variable color on the server
    {
        this.color = color;
    }
}
