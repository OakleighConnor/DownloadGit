using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SettingsHandler : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    void Start() // Calls methods setting settings that are stored in the PlayerPrefs
    {
        //LoadVolume();
    }

    public void SetMusicSliderValue(float value)
    {
        musicSlider.value = value;
    }
    
    public void SetSFXSliderValue(float value)
    {
        sfxSlider.value = value;
    }
}
