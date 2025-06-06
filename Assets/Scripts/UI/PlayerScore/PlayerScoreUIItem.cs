using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.UI;

public class PlayerScoreUIItem : MonoBehaviour
{
    public TextMeshProUGUI nicknameText;
    public TextMeshProUGUI scoreText;
    public Image scoreBackground;
    void Awake()
    {
        scoreText.text = 0.ToString();
        scoreBackground.color = GetComponent<CustomisationHandler>().color;
    }
}
