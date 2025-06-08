using Fusion;
using Player;
using UnityEngine;

public class PlayerScoreManager : NetworkBehaviour
{
    [SerializeField] GameSettings gameSettings;
    [Networked, HideInInspector] public PlayerScoreUIItem scoreUI { get; set; }
    [Networked, HideInInspector] public NetworkObject uiParent { get; set; }
    public string nickname;

    const byte startingScore = 0;

    [Networked, OnChangedRender(nameof(OnScoreUpdated))] public byte score { get; set; }

    [Networked, OnChangedRender(nameof(OnColorUpdated))] public Color color { get; set; }

    public void AssignScoreUIValues(PlayerScoreUIItem scoreUI, NetworkObject uiParent)
    {
        this.scoreUI = scoreUI;
        this.uiParent = uiParent;
        
        scoreUI.parent = uiParent;
        scoreUI.color = GetComponent<CustomisationHandler>().color;
        scoreUI.nickname = GetComponent<NicknameManager>().nickname.ToString();
    }

    public override void Spawned()
    {
        score = startingScore;
    }

    public void IncreaseScore() // Calls server to increase the score
    {
        Debug.Log("Score Increased");

        RPC_UpdateScore(score + 1, nickname);
    }
    public void DecreaseScore() // Calls server to decrease the score
    {
        if (score! <= 0)
        {
            RPC_UpdateScore(score--, nickname);
        }
        else
        {
            Debug.LogWarning("Attempting to decrease score when score is already equal to 0");
        }
    }

    // ChangeDetector
    void OnScoreUpdated() // Called when score of the player is changed
    {
        scoreUI.scoreUI.text = score.ToString();

        if (score >= gameSettings.winRequirement)
        {
            FindAnyObjectByType<PauseMenuHandler>().PlayerWin(nickname);
            PlayerScript[] players = FindObjectsByType<PlayerScript>(FindObjectsSortMode.None);
            foreach (PlayerScript player in players)
            {
                player.gameOver = true;
            }
        }
    }
    void OnColorUpdated() // Called when color of player is changed
    {
        scoreUI.scoreBackground.color = color;
    }

    // RPCs
    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_UpdateScore(int score, string winner, RpcInfo info = default) // Called by clients to update server with score
    {
        Debug.Log($"[RPC] UpdateScore {score}");

        this.score = (byte)score;
    }
}
