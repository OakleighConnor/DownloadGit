using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;

public class SessionListUIHandler : MonoBehaviour
{
    public TextMeshProUGUI statusText;
    public GameObject sessionItemListPF;
    public VerticalLayoutGroup verticalLayoutGroup;
    public GameObject createSessionButton;

    void Awake()
    {
        ClearList();
        createSessionButton.gameObject.SetActive(false);
    }

    public void ClearList()
    {
        // Delete all children of the vertical layout group
        foreach (Transform child in verticalLayoutGroup.transform)
        {
            Destroy(child.gameObject);
        }

        // Hide the status message
        statusText.gameObject.SetActive(false);
    }

    public void AddToList(SessionInfo sessionInfo)
    {
        //Add a new item to the list
        SessionInfoListUIItem addedSessionInfoListUIItem = Instantiate(sessionItemListPF, verticalLayoutGroup.transform).GetComponent<SessionInfoListUIItem>();

        addedSessionInfoListUIItem.SetInformation(sessionInfo);

        // Hook up events
        addedSessionInfoListUIItem.OnJoinSession += AddedSessionInfoListUIItem_OnJoinSession;
        
        createSessionButton.SetActive(true);
    }

    private void AddedSessionInfoListUIItem_OnJoinSession(SessionInfo sessionInfo)
    {
        createSessionButton.SetActive(true);
        NetworkRunnerHandler networkRunnerHandler = FindAnyObjectByType<NetworkRunnerHandler>();

        networkRunnerHandler.JoinGame(sessionInfo.Name);

        MainMenuUIHandler mainMenuUIHandler = FindAnyObjectByType<MainMenuUIHandler>();
        mainMenuUIHandler.OnJoiningServer();
    }

    public void OnNoSessionFound()
    {
        ClearList();
        Debug.Log("No sessions found");

        statusText.gameObject.SetActive(true);
        statusText.text = "No game session found";
        createSessionButton.SetActive(true);
    }

    public void OnLookingForGameSessions()
    {
        ClearList();

        statusText.text = "Connecting to server";
        statusText.gameObject.SetActive(true);
    }
}
