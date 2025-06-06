using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Player;
using UnityEngine.UI;
public class CustomisationHandler : NetworkBehaviour
{
    [SerializeField] Renderer playerBody;
    int player;

    [Networked, OnChangedRender(nameof(OnAppearanceChanged))]
    public Color color {get; set;}
    void Awake()
    {
        bool localPlayer = GetComponent<PlayerScript>().localPlayer;
        Debug.Log(localPlayer);

        if (localPlayer) player = 2;
        else player = 1;
    }
    public void ChangeColor(Color color, byte player) // Changes the Networked color variable
    {
        if (HasInputAuthority && player == this.player) // Only update the color variable of the player who clicked the button
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
        OnAppearanceChanged();

        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            if (HasInputAuthority)
            {
                LobbyUIHandler lobbyHandler = FindAnyObjectByType<LobbyUIHandler>();
                if (lobbyHandler != null) lobbyHandler.customisations.Add(this);

                RPC_SetColor(GetRandomPlayerColor(lobbyHandler.colorButtonSettingP1));
            }

            Debug.Log("Lobby scene");
        }
    }
    Color GetRandomPlayerColor(GameObject colorButtonsParent) // Gets a random color from the colors the player can choose from
    {
        List<Color> colors = new List<Color>();
        Button[] colorButtons = colorButtonsParent.GetComponentsInChildren<Button>();

        foreach (Button button in colorButtons) // Adds the color of each button within the parent of the color buttons to the list
        {
            colors.Add(button.GetComponent<Image>().color);
        }

        return colors[Random.Range(0, colors.Count)];
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
