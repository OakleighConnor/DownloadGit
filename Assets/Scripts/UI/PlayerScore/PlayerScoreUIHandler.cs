using UnityEngine;
using Fusion;
using UnityEngine.UI;

public class PlayerScoreUIHandler : MonoBehaviour
{
    public GameObject scoreItemPF;
    public GameObject horizontalLayoutGroup;
    public PlayerScoreUIItem CreatePlayerScoreItem()
    {
        return Instantiate(scoreItemPF, horizontalLayoutGroup.transform).GetComponent<PlayerScoreUIItem>();
    }
}
