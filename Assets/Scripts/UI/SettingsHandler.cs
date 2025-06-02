using UnityEngine;
using UnityEngine.UI;

public class SettingsHandler : MonoBehaviour
{
    [Header("Audio")]
    AudioManager am;
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    void Start() // Calls methods setting settings that are stored in the PlayerPrefs
    {
        am = FindAnyObjectByType<AudioManager>();

        if (PlayerPrefs.HasKey("masterVolume"))
        {
            LoadVolume();
        }
    }
    public void SetMasterVolume()
    {
        am.SetMasterVolume(masterSlider.value);
    }
    public void SetMusicVolume()
    {
        am.SetMusicVolume(musicSlider.value);
    }
    public void SetSFXVolume()
    {
        am.SetSFXVolume(sfxSlider.value);
    }
    void LoadVolume()
    {
        masterSlider.value = PlayerPrefs.GetFloat("masterVolume");
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume");
    }
}
