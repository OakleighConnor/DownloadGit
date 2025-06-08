using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class MainMenuUIHandler : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] GameSettings gameSettings;
    [SerializeField] Toggle localMultiplayer;

    [Header("Animation")]
    [SerializeField] Animator anim;
    
    [Header("Panels")]
    [SerializeField] GameObject mainMenuPanel;
    [SerializeField] GameObject sessionBrowserPanel;
    [SerializeField] GameObject createSessionPanel;
    [SerializeField] GameObject statusPanel;
    [SerializeField] GameObject settingsPanel;

    [Header("WinnerDisplay")]
    [SerializeField] TMP_Text winnerAnnouncement;

    [Header("CreateSessionSettings")]
    public TMP_Text diskWinRequirementDisplay;
    public TMP_Text publicity;

    
    [Header("Player Settings")]
    public TMP_InputField nameField;

    [Header("Join Settings")]
    public TMP_InputField codeInputText;

    [Header("References")]
    public CodeManager codeManager;

    void Start() // Sets the Player's name to the PlayerPref PlayerNickname and ensures that multiplayer is disabled by default
    {
        if (PlayerPrefs.HasKey("DiskWinRequirement")) gameSettings.winRequirement = PlayerPrefs.GetInt("DiskWinRequirement");
        else gameSettings.winRequirement = 5;
        diskWinRequirementDisplay.text = gameSettings.winRequirement.ToString();

        if (PlayerPrefs.HasKey("SessionPublicity")) publicity.text = PlayerPrefs.GetString("SessionPublicity");
        if (PlayerPrefs.HasKey("PlayerNickname")) nameField.text = PlayerPrefs.GetString("PlayerNickname");

        Debug.Log("Start");
        ChangePanel(mainMenuPanel, 1);

        ToggleLocalMultiplayer(false);
    }

    public void OnQuit()
    {
        Application.Quit();
    }
    void ChangePanel(GameObject activePanel, int cameraPos) // Hides all the Panels
    {
        mainMenuPanel.SetActive(false);
        sessionBrowserPanel.SetActive(false);
        createSessionPanel.SetActive(false);
        statusPanel.SetActive(false);
        settingsPanel.SetActive(false);

        activePanel.SetActive(true);
        anim.SetInteger("CameraPos", cameraPos);
    }
    public void OnFindGameClicked() // Sets the PlayerNickname PlayerPrefs, joins a NetworkRunner to a lobby, opens the SessionBrowserPanel
    {
        PlayerPrefs.SetString("PlayerNickname", nameField.text);
        PlayerPrefs.Save();

        NetworkRunnerHandler networkRunnerHandler = FindAnyObjectByType<NetworkRunnerHandler>();

        networkRunnerHandler.OnJoinLobby();

        ChangePanel(sessionBrowserPanel, 2);
        FindFirstObjectByType<SessionListUIHandler>().OnLookingForGameSessions();
    }
    public void OpenCreateSessionPanel() // Create Session Button inside of the SessionBrowserPanel
    {
        ChangePanel(createSessionPanel, 1);
    }
    public void OpenMainMenuPanel() // Create Session Button inside of the SessionBrowserPanel
    {
        ChangePanel(mainMenuPanel, 1);
    }

    public void TogglePublicity()
    {
        if (publicity.text == "Public") publicity.text = "Private";
        else publicity.text = "Public";
    }

    public void IncreaseWinRequirement()
    {
        if (gameSettings.winRequirement >= 99) gameSettings.winRequirement = 1;
        else gameSettings.winRequirement++;
        diskWinRequirementDisplay.text = gameSettings.winRequirement.ToString();
    }
    public void DecreaseWinRequirement()
    {
        if (gameSettings.winRequirement <= 1) gameSettings.winRequirement = 99;
        else gameSettings.winRequirement--;
        diskWinRequirementDisplay.text = gameSettings.winRequirement.ToString();
    }
    public void ToggleSettingsPanel() // Toggles between the settings and the menu panels
    {
        if (settingsPanel.activeSelf) ChangePanel(mainMenuPanel, 1); // If settings open, go to main menu
        else ChangePanel(settingsPanel, 2); // If settings close, go to settings
    }

    public void OnStartNewSessionClicked() // Creates a new Session in the NetworkRunnerHandler and changes the panel to the loading panel
    {
        PlayerPrefs.SetInt("DiskWinRequirement", gameSettings.winRequirement);
        PlayerPrefs.SetString("SessionPublicity", publicity.text);
        PlayerPrefs.Save();

        NetworkRunnerHandler networkRunnerHandler = FindAnyObjectByType<NetworkRunnerHandler>();

        networkRunnerHandler.CreateGame(codeManager.GenerateCode(), "Lobby", publicity.text); // Generates a random code in the helper

        ChangePanel(statusPanel, 3);
    }
    public void OnJoinNewSessionClicked() // Attempts to join the player to a session with the code entered into the code InputField
    {
        if (codeInputText.text.Length != 6)
        {
            codeInputText.text = "INVALID";
            return;
        }

        if (!codeManager.CompareCodeToSessionList(codeInputText.text)) return;

        NetworkRunnerHandler networkRunnerHandler = FindAnyObjectByType<NetworkRunnerHandler>();

        networkRunnerHandler.JoinGame(codeInputText.text);

        ChangePanel(statusPanel, 3);
    }
    public void OnJoiningServer() // Sets the active panel to the StatusPanel
    {
        ChangePanel(statusPanel, 3);
    }

    public void OnToggleLocalMultiplayer()
    {
        ToggleLocalMultiplayer(localMultiplayer);
    }

    void ToggleLocalMultiplayer(bool state) // Sets the LocalPlay PlayerPref
    {
        if (state == true)
        {
            Debug.Log("Enable Local Play");
            PlayerPrefs.SetInt("LocalPlay", 1); // true
        }
        else
        {
            Debug.Log("Disable Local Play");
            PlayerPrefs.SetInt("LocalPlay", 0); // false
        }
        PlayerPrefs.Save();
        Debug.Log(PlayerPrefs.GetInt("LocalPlay"));
    }
}