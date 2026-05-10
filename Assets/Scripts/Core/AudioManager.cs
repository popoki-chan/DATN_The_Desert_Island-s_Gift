using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Settings")]
    public float musicVolume = 0.7f;
    public float sfxVolume = 1f;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    void Start()
    {
        if (musicSource != null) musicSource.volume = musicVolume;
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic() => musicSource?.Stop();

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume * volumeScale);
    }
}

