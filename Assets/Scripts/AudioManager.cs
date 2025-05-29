using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Music")]


    [Header("SFX")]


    [Header("UI SFX")]
    public AudioClip[] buttonSounds;

    [Header("AudioMixer")]
    [SerializeField] AudioMixer audioMixer;

    [Header("AudioSources")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxSource;

    void Awake() // Makes the AudioManager object a singleton
    {
        if(instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(() => PlayRandomUISFX());
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("musicVolume"))
        {
            LoadVolume();
        }
        else
        {
            //SetMusicVolume();
            //SetSFXVolume();
        }
    }

    public void RestartMusic()
    {
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void PlayRandomUISFX() // Plays a random sound effect from the buttonSounds array
    {
        AudioClip buttonSFX = buttonSounds[Random.Range(1, buttonSounds.Length)];
        sfxSource.PlayOneShot(buttonSFX);
    }

    public void PlayMusic(AudioClip music) // Plays the music passed through the method
    {
        musicSource.clip = music;
        musicSource.Play();
    }

    public void SetMusicVolume(float volume) // Sets and saves music volume
    {
        audioMixer.SetFloat("music", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("musicVolume", volume);
    }
    public void SetSFXVolume(float volume) // Sets and saves sfx volume
    {
        audioMixer.SetFloat("sfx", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("sfxVolume", volume);
    }
    public void LoadVolume() // Loads saved volumes and sets them
    {
        /*musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume");*/

        SetMusicVolume(PlayerPrefs.GetFloat("musicVolume"));
        SetSFXVolume(PlayerPrefs.GetFloat("sfxVolume"));
    }
}
