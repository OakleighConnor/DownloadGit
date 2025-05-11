using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class LobbyUIHandler : NetworkBehaviour, IPlayerJoined
{
    [HideInInspector] public List<PlayerReadyUpHandler> prhs = new List<PlayerReadyUpHandler>();
    [HideInInspector] public List<CustomisationHandler> customisations = new List<CustomisationHandler>();

    [Header("SessionSettings")]
    [Networked, OnChangedRender (nameof(OnPublicityChanged))] 
    public bool sessionPublic {get; set;}

    [Header("Ready")]
    public bool isReady;
    public TickTimer countdownTickTimer = TickTimer.None;
    [Networked, OnChangedRender (nameof(OnCountdownChanged))] 
    public byte countdown {get; set;}
    public byte countdownDuration = 1;
    [Header("Game Settings")]
    public GameSettings gs;
    [Networked, OnChangedRender (nameof(OnWinRequirementChanged))] 
    public int winRequirement {get; set;} // Amount of disks required to win

    [Header("UI")]
    public GameObject[] colorButtons;
    public GameObject[] settingsButtons;
    public GameObject publicityButton;
    public TMP_Text winRequirementValue;
    public TMP_Text countdownText;
    public TMP_Text readyButtonText;
    void Start()
    {
        countdownText.text = " ";

        foreach (GameObject button in colorButtons)
        {
            button.GetComponent<Button>().onClick.AddListener(() => ChangeColor(button.GetComponent<Image>().color)); // Passes the color of the button through the ChangeColor method
        }
    }

    void Update()
    {
        if(countdownTickTimer.Expired(Runner))
        {
            StartGame();

            countdownTickTimer = TickTimer.None;
        }   
        else if (countdownTickTimer.IsRunning)
        {
            countdown = (byte)countdownTickTimer.RemainingTime(Runner);
        }
    }
    public void PlayerJoined(PlayerRef player)
    {
        if (Runner.IsServer)
        {
            countdownTickTimer = TickTimer.None;
            countdown = 0;
        }
    }
    public override void Spawned()
    {
        ToggleSettings(HasStateAuthority); // Allows settings to be edited if state authority. Else settings cannot be altered
        Runner.SessionInfo.Properties.TryGetValue("Public", out var isSessionPublic);
        sessionPublic = isSessionPublic;
        OnPublicityChanged();
    }
    public void ToggleSettings(bool state) // Toggles all UI elements that change settings
    {
        foreach (GameObject button in settingsButtons)
        {
            button.SetActive(state);
        }

        publicityButton.GetComponent<Button>().enabled = state;
    }
    public void OnTogglePublicity() // Toggles the Public SessionProperty in the SessionInfo
    {
        Runner.SessionInfo.Properties.TryGetValue("Public", out var wasPublic);
        sessionPublic = !wasPublic; // Set the publicity of the session to the opposite of what it was
        Runner.SessionInfo.UpdateCustomProperties(new Dictionary<string, SessionProperty>() {{"Public", sessionPublic}}); // Set the publicity to the opposite of what it was
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
            winRequirement--;
        }
    }
    public void ChangeColor(Color color) // Sets the color of the player to the color of the button clicked
    {
        Debug.Log($"ChangeColor({color})");
        foreach (CustomisationHandler customisation in customisations)
        {
            customisation.ChangeColor(color);
        }
    }

    void StartGame()
    {
        Runner.SessionInfo.IsOpen = false;

        GameObject[] gameObjectsToTransfer = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject gameObject in gameObjectsToTransfer)
        {
            DontDestroyOnLoad(gameObject);
        }

        Runner.LoadScene("Level");
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
    public void OnReady()
    {
        if(isReady) isReady = false;
        else isReady = true;

        if(isReady) readyButtonText.text = "Cancel";
        else readyButtonText.text = "Ready";

        foreach(PlayerReadyUpHandler prh in prhs)
        {
            prh.OnReady(isReady);
        }
    }
    public void KickPlayer()
    {

    }
    void OnCountdownChanged()
    {
        if(countdown == 0)
        {
            countdownText.text = "";
        }
        else
        {
            countdownText.text = $"Game Session starts in {countdown + 1}";
        }
    }
    void OnPublicityChanged() // Updates the PublicityButton to suit the session's publicity
    {
        // Get components of the button that will be changed
        Image buttonImage = publicityButton.GetComponent<Image>();
        TMP_Text buttonText = publicityButton.GetComponentInChildren<TMP_Text>();
        
        if(sessionPublic) // If the button wasn't public, make it public
        {
            buttonImage.color = Color.green;
            buttonText.text = "PUBLIC";
        }
        else // If the button was public, make it private
        {
            buttonImage.color = Color.red;
            buttonText.text = "PRIVATE";
        }
    }
}
