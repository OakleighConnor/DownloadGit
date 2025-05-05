using Fusion;
using UnityEngine;

public class PlayerScoreManager : NetworkBehaviour
{
    PlayerScoreUIHandler psh;
    PlayerScoreUIItem psi;

    const byte startingScore = 0;
    const byte winningScore = 5;

    [Networked, OnChangedRender (nameof(OnScoreUpdated))] 
    public byte score {get; set;}

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        psi.scoreText.text = score.ToString();   
    }
    public override void Spawned()
    {
        psh = FindAnyObjectByType<PlayerScoreUIHandler>();
        psi = psh.CreatePlayerScoreItem();

        score = startingScore;
    }

    public void IncreaseScore()
    {
        Debug.Log("Score Increased");
        score ++;
    }

    public void DecreaseScore()
    {
        if(score <= 0)
        {
            Debug.Log("Score is already 0. Score wasn't decreased");
            return;
        }

        score --;
        Debug.Log("Score Decreased");
    }

    void OnScoreUpdated() // Called when score of the player is changed
    {
        psi.UpdateScore(score);
    }
}
