using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainMenuUIHandler : MonoBehaviour
{
    [Header("Panels")]
    public GameObject playerDetailsPanel;
    public GameObject sessionBrowserPanel;
    public GameObject statusPanel;
    
    [Header("Player Settings")]
    public TMP_InputField nameField;

    [Header("Join Settings")]
    public TMP_InputField codeInputText;

    [Header("References")]
    public CodeManager codeManager;

    void Start() // Sets the Player's name to the PlayerPref PlayerNickname and ensures that multiplayer is disabled by default
    {
        if(PlayerPrefs.HasKey("PlayerNickname"))
        {
            nameField.text = PlayerPrefs.GetString("PlayerNickname");
        }

        ToggleLocalMultiplayer(false);
    }
    void HideAllPanels() // Hides all the Panels
    {
        playerDetailsPanel.SetActive(false);
        sessionBrowserPanel.SetActive(false);
        statusPanel.SetActive(false);
    }
    public void OnFindGameClicked() // Sets the PlayerNickname PlayerPrefs, joins a NetworkRunner to a lobby, opens the SessionBrowserPanel
    {
        PlayerPrefs.SetString("PlayerNickname", nameField.text);
        PlayerPrefs.Save();

        //GameManager.instance.playerNickname = playerNicknameField.text;

        NetworkRunnerHandler networkRunnerHandler = FindAnyObjectByType<NetworkRunnerHandler>();

        networkRunnerHandler.OnJoinLobby();

        HideAllPanels();

        sessionBrowserPanel.gameObject.SetActive(true);
        FindFirstObjectByType<SessionListUIHandler>().OnLookingForGameSessions();
    }
    public void OnStartNewSessionClicked() // Creates a new Session in the NetworkRunnerHandler and changes the panel to the loading panel
    {
        NetworkRunnerHandler networkRunnerHandler = FindAnyObjectByType<NetworkRunnerHandler>();

        networkRunnerHandler.CreateGame(codeManager.GenerateCode(), "Lobby"); // Generates a random code in the helper

        HideAllPanels();

        statusPanel.gameObject.SetActive(true);
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

        HideAllPanels();

        statusPanel.gameObject.SetActive(true);
    }
    public void OnJoiningServer() // Sets the active panel to the StatusPanel
    {
        HideAllPanels();

        statusPanel.gameObject.SetActive(true);
    }

    public void ToggleLocalMultiplayer(bool state) // Sets the LocalPlay PlayerPref
    {
        // false = 0
        // true = 1
        if(state == true)
        {
            Debug.Log("Enable Local Play");
            PlayerPrefs.SetInt("LocalPlay", 1);
        }
        else
        {
            Debug.Log("Disable Local Play");
            PlayerPrefs.SetInt("LocalPlay", 0);
        }
        PlayerPrefs.Save();
        Debug.Log(PlayerPrefs.GetInt("LocalPlay"));
    }
}