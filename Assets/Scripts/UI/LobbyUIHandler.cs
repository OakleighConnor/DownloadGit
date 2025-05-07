using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.UI;

public class LobbyUIHandler : NetworkBehaviour
{
    public GameObject[] colorButtons;
    public GameObject[] settingsButtons;
    [Header("Ready")]
    public bool isReady;
    [Header("Game Settings")]
    public GameSettings gs;
    [Networked, OnChangedRender (nameof(OnWinRequirementChanged))] 
    public int winRequirement {get; set;} // Amount of disks required to win

    [Header("Game Settings UI")]
    public TMP_Text winRequirementValue;

    [Header("Color")]
    [Networked, OnChangedRender (nameof(OnChangeColor))] 
    public Color playerColor {get; set;} // The color of the player
    void Awake()
    {
        foreach (GameObject button in colorButtons)
        {
            button.GetComponent<Button>().onClick.AddListener(() => ChangeColor(button.GetComponent<Image>().color)); // Passes the color of the button through the ChangeColor method
        }
    }
    public override void Spawned()
    {
        ToggleSettings(HasStateAuthority); // Allows settings to be edited if state authority. Else settings cannot be altered
    }
    public void ToggleSettings(bool state) // Toggles all UI elements that change settings
    {
        foreach (GameObject button in settingsButtons)
        {
            button.SetActive(state);
        }
    }
    public void IncreaseWinRequirement()
    {
        Debug.Log("Increasing Win Requirement Value");

        if(winRequirement >= 99)
        {
            winRequirement = 1;
        }
        else
        {
            winRequirement++;
        }
    }
    public void DecreaseWinRequirement()
    {
        Debug.Log("Decreasing Win Requirement Value");

        if(winRequirement <= 1)
        {
            winRequirement = 99;
        }
        else
        {
            winRequirement++;
        }
    }
    void ChangeColor(Color color) // Sets the color of the player to the color of the button clicked
    {
        // Change the player's color
    }

    // OnChangedRender Methods
    void OnWinRequirementChanged() // Updates the UI text to all clients, and updates the game settings scriptable object, if host
    {
        winRequirementValue.text = winRequirement.ToString();

        if(HasStateAuthority)
        {
            gs.winRequirement = winRequirement;
        }
    }
    void OnChangeColor() // Changes the color of the player
    {
        Debug.Log("Player's color changed");
        // Change the color of the player
    }

    void OnReady()
    {
        if(isReady) isReady = false;
        else isReady = true;

        // Make start game
    }
}
