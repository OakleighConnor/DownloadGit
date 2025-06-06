using UnityEngine;

public class HowToPlayHandler : MonoBehaviour
{
    
    [SerializeField] GameObject howToPlayCanvas;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        howToPlayCanvas.SetActive(true);
    }
    public void CloseHowToPlayCanvas()
    {
        howToPlayCanvas.SetActive(false);
    }
}
