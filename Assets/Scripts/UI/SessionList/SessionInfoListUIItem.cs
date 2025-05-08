using System;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionInfoListUIItem : MonoBehaviour
{
    public TextMeshProUGUI sessionNameText;
    public TextMeshProUGUI playerCountText;
    public Button joinButton;

    SessionInfo sessionInfo;

    // Events
    public event Action<SessionInfo> OnJoinSession;

    public void SetInformation(SessionInfo sessionInfo)
    {
        this.sessionInfo = sessionInfo;

        // Get Properties of the Session from SessionInfo
        sessionInfo.Properties.TryGetValue("PlayerCount", out var playerCountProperty);
        sessionInfo.Properties.TryGetValue("SessionName", out var sessionNameProperty);

        // Convert var to variable type
        string sessionName = sessionNameProperty;
        int playerCount = playerCountProperty;

        // Assign UI Text
        sessionNameText.text = sessionName;
        playerCountText.text = $"{playerCount.ToString()}/{sessionInfo.MaxPlayers.ToString()}";

        // Join Button
        bool isJoinButtonActive = true;
        if(playerCount >= sessionInfo.MaxPlayers)
        {
            isJoinButtonActive = false;
        }
        joinButton.gameObject.SetActive(isJoinButtonActive);
    }

    public void OnClick()
    {
        //Invoke the join session event
        OnJoinSession?.Invoke(sessionInfo);
    }
}

