using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

public class LobbyUIHandler : NetworkBehaviour, IPlayerJoined
{
    [HideInInspector] public List<PlayerReadyUpHandler> prhs = new List<PlayerReadyUpHandler>();
    [HideInInspector] public List<CustomisationHandler> customisations = new List<CustomisationHandler>();

    [Header("SessionSettings")]
    [Networked, OnChangedRender (nameof(OnPublicityChanged))] 
    [SerializeField] bool sessionPublic {get; set;}

    [Header("Ready")]
    [SerializeField] bool isReady;
    public TickTimer countdownTickTimer = TickTimer.None;
    [Networked, OnChangedRender (nameof(OnCountdownChanged))] 
    public byte countdown {get; set;}
    public byte countdownDuration = 1;
    [Header("Game Settings")]
    [SerializeField] GameSettings gs;
    [Networked, OnChangedRender (nameof(OnWinRequirementChanged))] 
    [SerializeField] int winRequirement {get; set;} // Amount of disks required to win

    [Header("Buttons")]
    [SerializeField] GameObject publicityButton;
    [SerializeField] GameObject settingsPanel;
    [SerializeField] GameObject settingsButton;
    [SerializeField] GameObject readyButton;
    [SerializeField] GameObject[] settingsButtons;

    [Header("Customisation UI")]
    [SerializeField] TMP_Text colorButtonSettingTextP1;
    public GameObject colorButtonSettingP1;
    public GameObject colorButtonSettingP2;

    [Header("Text")]
    [SerializeField] TMP_Text winRequirementValue;
    [SerializeField] TMP_Text countdownText;
    [SerializeField] TMP_Text readyButtonText;
    [SerializeField] TMP_Text sessionCode;
    void Awake()
    {
        countdownText.text = " ";

        settingsPanel.SetActive(false);

        // Add ChangeColor method as a listener of OnClick on P1's buttons
        Button[] colorButtons = colorButtonSettingP1.GetComponentsInChildren<Button>();
        foreach (Button button in colorButtons)
        {
            Color buttonColor = button.GetComponent<Image>().color;
            button.onClick.AddListener(() => ChangeColor(buttonColor, 1)); // Passes the color of the button through the ChangeColor method
        }

        colorButtonSettingP2.SetActive(false);
        if (PlayerPrefs.GetInt("LocalPlay") == 1)
        {
            AddLocalPlayerSettings();
        }
    }

    void AddLocalPlayerSettings()
    {
        colorButtonSettingTextP1.text = "Player 1 Colors"; // Updates P1's text to specify P1 when there are two players

        colorButtonSettingP2.SetActive(true);

        // Add ChangeColor method as a listener of OnClick on P2's buttons
        Button[] colorButtons = colorButtonSettingP2.GetComponentsInChildren<Button>();
        foreach (Button button in colorButtons)
        {
            Color buttonColor = button.GetComponent<Image>().color;
            button.onClick.AddListener(() => ChangeColor(buttonColor, 2)); // Passes the color of the button through the ChangeColor method
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
        sessionCode.text = "CODE: " + Runner.SessionInfo.Name;
        ToggleSettings(HasStateAuthority); // Allows settings to be edited if state authority. Else settings cannot be altered
        Runner.SessionInfo.Properties.TryGetValue("Public", out var isSessionPublic);
        sessionPublic = isSessionPublic;
        OnPublicityChanged();

        if (Runner.IsServer)
        {
            winRequirement = gs.winRequirement;
        }
        winRequirementValue.text = winRequirement.ToString();
    }
    public void ToggleSettings(bool state) // Toggles all UI elements that change settings
    {
        foreach (GameObject button in settingsButtons)
        {
            button.SetActive(state);
        }

        publicityButton.GetComponent<Button>().enabled = state;
    }
    public void OnToggleSettingsPanel()
    {
        if (settingsPanel.activeSelf == true)
        {
            settingsPanel.SetActive(false);
            readyButton.SetActive(true);
            settingsButton.SetActive(true);
        }
        else
        {
            settingsPanel.SetActive(true);
            readyButton.SetActive(false);
            settingsButton.SetActive(false);
        }
    }
    public void OnTogglePublicity() // Toggles the Public SessionProperty in the SessionInfo
    {
        Runner.SessionInfo.Properties.TryGetValue("Public", out var wasPublic);
        sessionPublic = !wasPublic; // Set the publicity of the session to the opposite of what it was
        Runner.SessionInfo.UpdateCustomProperties(new Dictionary<string, SessionProperty>() { { "Public", sessionPublic } }); // Set the publicity to the opposite of what it was
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
    public void ChangeColor(Color color, byte player) // Sets the color of the player to the color of the button clicked
    {
        Debug.Log($"ChangeColor({color})");
        foreach (CustomisationHandler customisation in customisations)
        {
            customisation.ChangeColor(color, player);
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

        settingsButton.SetActive(!isReady);

        if (settingsPanel.activeSelf) settingsPanel.SetActive(false);

        if (isReady) readyButtonText.text = "Cancel";
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
        if(!countdownTickTimer.IsRunning)
        {
            countdownText.gameObject.SetActive(false);
            countdownText.text = "";
        }
        else
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = $"Game Session starts in {countdown}";
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
