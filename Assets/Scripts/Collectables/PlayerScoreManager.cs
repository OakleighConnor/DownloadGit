using Fusion;
using UnityEngine;

public class PlayerScoreManager : NetworkBehaviour
{
    [SerializeField] GameSettings gameSettings;
    [HideInInspector] public PlayerScoreUIItem scoreUI;
    public string nickname;

    const byte startingScore = 0;

    [Networked, OnChangedRender(nameof(OnScoreUpdated))]
    public byte score { get; set; }

    public void AssignScoreUIValues(PlayerScoreUIItem scoreUI)
    {
        this.scoreUI = scoreUI;
        scoreUI.nicknameText.text = nickname;
        scoreUI.scoreBackground.color = GetComponent<CustomisationHandler>().color;
    }

    public override void Spawned()
    {
        score = startingScore;
    }

    public void IncreaseScore() // Calls server to increase the score
    {
        Debug.Log("Score Increased");
        
        RPC_UpdateScore(score + 1);
    }
    public void DecreaseScore() // Calls server to decrease the score
    {
        if (score! <= 0)
        {
            RPC_UpdateScore(score--);
        }
        else
        {
            Debug.LogWarning("Attempting to decrease score when score is already equal to 0");
        }
    }

    void OnScoreUpdated() // Called when score of the player is changed
    {
        scoreUI.scoreText.text = score.ToString();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_UpdateScore(int score, RpcInfo info = default) // Called by clients to update server with score
    {
        Debug.Log($"[RPC] UpdateScore {score}");

        this.score = (byte)score;

        if (score >= gameSettings.winRequirement)
        {
            PlayerScoreUIHandler uiScoreHandler = FindAnyObjectByType<PlayerScoreUIHandler>();
            uiScoreHandler.EndGame(nickname);
        }
    }
}
