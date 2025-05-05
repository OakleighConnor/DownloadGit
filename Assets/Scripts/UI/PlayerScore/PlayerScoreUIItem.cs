using UnityEngine;
using Fusion;
using TMPro;

public class PlayerScoreUIItem : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    void Awake()
    {
        scoreText.text = 0.ToString();
    }
    public void UpdateScore(byte score)
    {
        scoreText.text = score.ToString();
    }
}
