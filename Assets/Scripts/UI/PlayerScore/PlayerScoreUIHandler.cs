using UnityEngine;
using Fusion;
using UnityEngine.UI;
using Player;

public class PlayerScoreUIHandler : NetworkBehaviour
{
    public GameObject scoreItemPF;
    public NetworkObject horizontalLayoutGroup;
    public PlayerScoreManager[] playerScoreManagers;

    public override void Spawned()
    {
        if (HasStateAuthority) CreatePlayerScoreItems();
    }
    public void CreatePlayerScoreItems() // Creates a PlayerScoreItem for all active PlayerScoreManagers in the scene (one for each player)
    {
        playerScoreManagers = FindObjectsByType<PlayerScoreManager>(FindObjectsSortMode.None);

        foreach (PlayerScoreManager scoreManager in playerScoreManagers)
        {
            NetworkObject psmObj = Runner.Spawn(scoreItemPF);
            PlayerScoreUIItem uiItemScript = psmObj.GetComponent<PlayerScoreUIItem>();

            scoreManager.AssignScoreUIValues(uiItemScript, horizontalLayoutGroup);
        }
    }
}
