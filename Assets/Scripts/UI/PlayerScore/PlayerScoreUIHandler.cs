using UnityEngine;
using Fusion;
using UnityEngine.UI;
using Player;

public class PlayerScoreUIHandler : NetworkBehaviour
{
    public GameObject scoreItemPF;
    public GameObject horizontalLayoutGroup;
    public PlayerScoreManager[] playerScoreManagers;

    public override void Spawned()
    {
        CreatePlayerScoreItems();
    }
    public void CreatePlayerScoreItems() // Creates a PlayerScoreItem for all active PlayerScoreManagers in the scene (one for each player)
    {
        playerScoreManagers = FindObjectsByType<PlayerScoreManager>(FindObjectsSortMode.None);

        foreach (PlayerScoreManager scoreManager in playerScoreManagers)
        {
            NetworkObject psmObj = Runner.Spawn(scoreItemPF);
            psmObj.transform.SetParent(horizontalLayoutGroup.transform);

            scoreManager.AssignScoreUIValues(psmObj.GetComponent<PlayerScoreUIItem>());
        }
    }

    public void EndGame(string playerName) // Ends the game once a player reaches the score required to win
    {
        Debug.Log($"Game has ended. Player {playerName} has won");
        FindAnyObjectByType<AudioManager>().winnerName = playerName;
        FindAnyObjectByType<AudioManager>().gameEnded = true;
        Runner.Shutdown(true, ShutdownReason.GameClosed);
    }
}
