using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerScoreManager : NetworkBehaviour
{
    PlayerScoreUIHandler psh;
    PlayerScoreUIItem psi;

    public GameSettings gameSettings;
    const byte startingScore = 0;

    [Networked, OnChangedRender (nameof(OnScoreUpdated))] 
    public byte score {get; set;}

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(SceneManager.GetActiveScene().name == "Lobby" || SceneManager.GetActiveScene().name == "Menu") return;
        psi.scoreText.text = score.ToString();   
    }
    public override void Spawned()
    {
        if(SceneManager.GetActiveScene().name == "Lobby" || SceneManager.GetActiveScene().name == "Menu") return;
        psh = FindAnyObjectByType<PlayerScoreUIHandler>();
        psi = psh.CreatePlayerScoreItem();

        score = startingScore;
    }

    void OnEnable()
    {
        
    }

    public void IncreaseScore()
    {
        Debug.Log("Score Increased");
        score ++;

        if(score >= gameSettings.winRequirement)
        {
            Debug.Log("Win");
        }
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
        if (psi == null) return;
        psi.UpdateScore(score);
    }
}
