using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class LevelSettings : NetworkBehaviour
{
    [Header("Pannels")]
    public GameObject playerScorePanel;
    public GameObject playerSettingsPanel;
    bool settingsOpen;
    public void Awake()
    {
        playerScorePanel.SetActive(!settingsOpen);
        playerSettingsPanel.SetActive(settingsOpen);
    }
    public void ToggleSettings() // Called when settings input is used
    {
        settingsOpen = !settingsOpen;

        // Toggle UI Panels
        playerScorePanel.SetActive(!settingsOpen);
        playerSettingsPanel.SetActive(settingsOpen);
    }
}