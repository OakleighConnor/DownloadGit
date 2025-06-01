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
    public AudioClip menuTheme;
    public AudioClip lobbyTheme;
    public AudioClip levelTheme;

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

        PlaySceneMusic(SceneManager.GetActiveScene().name);
    }

    void PlaySceneMusic(string scene)
    {
        if (scene == "Menu")
        {
            PlayMusic(menuTheme);
        }
        else if (scene == "Lobby")
        {
            PlayMusic(lobbyTheme);
        }
        else
        {
            PlayMusic(levelTheme);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //PlaySceneMusic(SceneManager.GetActiveScene().name);

        if (PlayerPrefs.HasKey("musicVolume"))
        {
            // Load the volume using the PlayerPrefs
            LoadVolume();
        }
        else
        {
            // Set the volume to max
            SetMasterVolume(1);
            SetMusicVolume(1);
            SetSFXVolume(1);
        }
    }

    void LoadVolume()
    {
        SetMasterVolume(PlayerPrefs.GetFloat("masterVolume"));
        SetMusicVolume(PlayerPrefs.GetFloat("musicVolume"));
        SetSFXVolume(PlayerPrefs.GetFloat("sfxVolume"));
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
    public void SetMasterVolume(float volume) // Sets and saves music volume
    {
        audioMixer.SetFloat("master", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("masterVolume", volume);
    }
    public void SetMusicVolume(float volume) // Sets and saves music volume
    {
        audioMixer.SetFloat("music", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("musicVolume", volume);
    }
    public void SetSFXVolume(float volume) // Sets and saves sfx volume
    {
        audioMixer.SetFloat("sfx", (Mathf.Log10(volume) + 0.5f) * 20);
        PlayerPrefs.SetFloat("sfxVolume", volume);
    }
}
