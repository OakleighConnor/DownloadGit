using Fusion;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class CollectableManager : NetworkBehaviour // Stores the score of the players
{
    public byte scoreP1;
    public byte scoreP2;
    public byte scoreP3;
    public byte scoreP4;

    [Networked, OnChangedRender (nameof(OnScoreUpdated))] 
    byte score {get; set;}
    const byte startingScore = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        score = startingScore;
    }

    void UpdateScore()
    {
        score += 1;
        Debug.Log($"{score}");
    }

    void OnScoreUpdated() // Updates the UI containing the player's score
    {
        Debug.Log("Score updated");
        Debug.Log(score);
    }
}
