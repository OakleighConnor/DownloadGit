using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SettingsHandler : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] AudioManager am;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;
    
    void Start() // Calls methods setting settings that are stored in the PlayerPrefs
    {
        LoadVolume();
    }
    public void SetMusicVolume() // Sets Music volume in the AudioManager to the slider's value
    {
        am.SetMusicVolume(musicSlider.value);
    }
    public void SetSFXVolume() // Sets SFX volume in the AudioManager to the slider's value
    {
        am.SetMusicVolume(sfxSlider.value);
    }
    void LoadVolume() // Loads the volume stored in the PlayerPrefs
    {
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume");

        am.SetMusicVolume(musicSlider.value);
        am.SetMusicVolume(sfxSlider.value);
    }
}
