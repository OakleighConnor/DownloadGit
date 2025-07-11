using Fusion;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PauseMenuHandler : NetworkBehaviour
{
    [Header("Pannels")]
    public GameObject playerScorePanel;
    public GameObject menuPanel;
    public GameObject settingsPanel;
    public GameObject playerWinPanel;

    [Header("PlayerWin")]
    public TextMeshProUGUI playerWinUI;
    public void Awake()
    {
        menuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        playerWinPanel.SetActive(false);
    }
    public void TogglePanel() // Changes settings panel to pause menu and enables/disables pause menu
    {
        if (playerWinPanel.activeSelf) return;
        if (menuPanel.activeSelf) menuPanel.SetActive(false);
        else if (settingsPanel.activeSelf) settingsPanel.SetActive(false);
        else menuPanel.SetActive(true);
    }
    public void ToggleSettings() // Toggles the state of the pause and settings panel
    {
        if (playerWinPanel.activeSelf)
        {
            settingsPanel.SetActive(false);
            menuPanel.SetActive(false);
        }
        else
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
            menuPanel.SetActive(!menuPanel.activeSelf);
        }
    }
    public void PlayerWin(string winner)
    {
        playerWinUI.text = $"{winner} has won the game";
        playerWinPanel.SetActive(true);
    }
    public void OnLeaveSession() // Disconnects user from the session
    {
        Debug.Log("Shutting down runner");
        Runner.Shutdown(true, ShutdownReason.Ok);
    }
}