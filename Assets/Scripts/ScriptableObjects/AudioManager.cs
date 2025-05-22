using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.Audio;
using System;

[CreateAssetMenu(fileName = "AudioManager", menuName = "Audio Manager/Audio Manager")]
public class AudioManager : ScriptableObject
{
    [Header("Music")]
    public AudioClip menu;
    public AudioClip lobby;
    public AudioClip level;

    [Header("SFX")]
    public AudioClip jumpSFX;

    [Header("Mixers")]
    public AudioMixer audioMixer;

    [Header("Audio sources")]
    [SerializeField] AudioSource musicSourcePF;
    [SerializeField] AudioSource sfxSourcePF;
    AudioSource musicSource;
    AudioSource sfxSource;
    void OnEnable() // Subscribes OnLoadScene to SceneManager.sceneLoaded
    {
        //SceneManager.sceneLoaded += OnLoadScene;
    }
    void OnLoadScene(Scene scene, LoadSceneMode mode) // Instantiates new audio sources and sets the music to what should play in the scene
    {
        InstantiateAudioSources();
        ChangeMusic(GetMusic(scene.name)); // Sets the music to the music that should be playing for the scene
    }
    AudioClip GetMusic(string scene) // Returns the music for the active scene. Pass through scene.name
    {
        if(scene == "Menu")
        {
            return menu;

        }
        else if (scene == "Lobby")
        {
            return lobby;

        }
        else if(scene == "Level")
        {
            return level;
        }
        else
        {
            Debug.LogError($"No music assigned for scene {scene}");
            return null;
        }
    }
    void InstantiateAudioSources() // If musicSource/sfxSource is null, instantiate a new one
    {
        if(musicSource == null) musicSource = Instantiate(musicSourcePF).GetComponent<AudioSource>();
        if(sfxSource == null) sfxSource = Instantiate(sfxSourcePF).GetComponent<AudioSource>();
    }
    public void ChangeMusic(AudioClip clip) // Changes the music to the AudioClip passed through the method.
    {
        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.Play();
    }
    public void PlaySFX(AudioClip clip) // Plays the SFX passed through the method.
    {
        sfxSource.PlayOneShot(clip);
    }
    public void SetMusicVolume(float value)
    {
        float volume = value;
        audioMixer.SetFloat("music", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("musicVolume", volume);
    }
    public void SetSFXVolume(float value)
    {
        float volume = value;
        audioMixer.SetFloat("sfx", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("sfxVolume", volume);
    }
}