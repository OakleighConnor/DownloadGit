using UnityEngine;
using TMPro;

public class MainMenuUIHandler : MonoBehaviour
{
    [Header("Panels")]
    public GameObject playerDetailsPanel;
    public GameObject sessionBrowserPanel;
    public GameObject statusPanel;
    
    [Header("Player Settings")]
    public TMP_InputField inputField;

    [Header("Join Settings")]
    public TMP_InputField codeInputText;

    [Header("References")]
    public CodeManager codeManager;
    void Start()
    {
        if(PlayerPrefs.HasKey("PlayerNickname"))
        {
            inputField.text = PlayerPrefs.GetString("PlayerNickname");
        }

        ToggleLocalMultiplayer(false);
    }

    void HideAllPanels()
    {
        playerDetailsPanel.SetActive(false);
        sessionBrowserPanel.SetActive(false);
        statusPanel.SetActive(false);
    }

    public void OnFindGameClicked()
    {
        PlayerPrefs.SetString("PlayerNickname", inputField.text);
        PlayerPrefs.Save();

        //GameManager.instance.playerNickname = playerNicknameField.text;

        NetworkRunnerHandler networkRunnerHandler = FindAnyObjectByType<NetworkRunnerHandler>();

        networkRunnerHandler.OnJoinLobby();

        HideAllPanels();

        sessionBrowserPanel.gameObject.SetActive(true);
        FindFirstObjectByType<SessionListUIHandler>().OnLookingForGameSessions();
    }

    public void OnStartNewSessionClicked()
    {
        NetworkRunnerHandler networkRunnerHandler = FindAnyObjectByType<NetworkRunnerHandler>();

        networkRunnerHandler.CreateGame(codeManager.GenerateCode(), "Level"); // Generates a random code in the helper

        HideAllPanels();

        statusPanel.gameObject.SetActive(true);
    }

    public void OnJoinNewSessionClicked()
    {
        if (codeInputText.text.Length != 6)
        {
            Debug.Log("Code isn't a real length");
            return;
        }

        if (!codeManager.CompareCodeToSessionList(codeInputText.text)) return;

        NetworkRunnerHandler networkRunnerHandler = FindAnyObjectByType<NetworkRunnerHandler>();

        networkRunnerHandler.JoinGame(codeInputText.text);

        HideAllPanels();

        statusPanel.gameObject.SetActive(true);
    }

    public void OnJoiningServer()
    {
        HideAllPanels();

        statusPanel.gameObject.SetActive(true);
    }

    public void ToggleLocalMultiplayer(bool state)
    {
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