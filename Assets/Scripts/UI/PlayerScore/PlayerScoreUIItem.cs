using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.UI;

public class PlayerScoreUIItem : NetworkBehaviour
{
    public TextMeshProUGUI nicknameUI;
    public TextMeshProUGUI scoreUI;
    public Image scoreBackground;

    [Networked, OnChangedRender(nameof(OnParentChanged))] public NetworkObject parent { get; set; }
    [Networked, OnChangedRender(nameof(OnColorChanged))] public Color color { get; set; }
    [Networked, OnChangedRender(nameof(OnNicknameChanged))] public NetworkString<_16> nickname { get; set; }


    void Awake()
    {
        scoreUI.text = 0.ToString();
    }

    public void SetNicknameText(string text)
    {
        nicknameUI.text = text;
    }

    void OnParentChanged()
    {
        transform.SetParent(parent.transform);
    }
    void OnColorChanged()
    {
        scoreBackground.color = color;
    }
    void OnNicknameChanged()
    {
        nicknameUI.text = nickname.ToString();
    }

    public override void Spawned()
    {
        OnParentChanged();
        OnColorChanged();
        OnNicknameChanged();
    }
}
