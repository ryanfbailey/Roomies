using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("SFX")]
    // Start delay
    public float startDelay = 6.5f;
    // Seek sfx
    public AudioClip startSFX;
    // Snatched sfx
    public AudioClip winSFX;

    [Header("Music")]
    // Menu music
    public AudioClip menuMusic;
    // Game music
    public AudioClip gameMusic;
    // Result music
    public AudioClip resultMusic;
    // Audio source
    private AudioSource _src;

    // Volumes
    [Range(0f, 1f)]
    public float defaultVolume = 1.0f;
    [Range(0f, 1f)]
    public float pauseVolume = 0.5f;

    // Add delegates
    protected virtual void Awake()
    {
        _src = gameObject.GetComponent<AudioSource>();
        if (_src == null)
        {
            _src = gameObject.AddComponent<AudioSource>();
        }
        _src.playOnAwake = false;
        _src.loop = true;
        _src.spatialBlend = 0f;
        _src.volume = defaultVolume;
        GameManager.onGameStateChange += OnGameStateChanged;
    }
    // Remove delegates
    protected virtual void OnDestroy()
    {
        GameManager.onGameStateChange -= OnGameStateChanged;
    }

    // State changed
    protected virtual void OnGameStateChanged(GameState newState, bool immediately)
    {
        // Set music
        switch (newState)
        {
            case GameState.Title:
            case GameState.PlayerSetup:
                PlayMusic(menuMusic, true);
                break;
            case GameState.GameIntro:
            case GameState.GameLoad:
            case GameState.GamePlay:
            case GameState.GamePause:
            case GameState.RoundComplete:
                PlayMusic(gameMusic, true);
                break;
            case GameState.MatchComplete:
                PlayMusic(resultMusic, true);
                break;
        }

        // Set volume
        if (newState == GameState.GamePause || newState == GameState.RoundComplete)
        {
            _src.volume = pauseVolume;
        }
        // Set volume
        else
        {
            _src.volume = defaultVolume;
        }

        // Wait intro
        if (newState == GameState.GameIntro)
        {
            Invoke("PlayStart", startDelay);
        }
        // Wait for results
        else if (newState == GameState.RoundComplete)
        {
            PlaySFX(winSFX, true);
        }
    }

    // Play start
    private void PlayStart()
    {
        PlaySFX(startSFX, true);
    }

    // Play music
    public void PlayMusic(AudioClip clip, bool loop)
    {
        if (_src.clip != clip)
        {
            _src.loop = loop;
            _src.clip = clip;
            _src.Play();
        }
    }

    // Play sfx
    public void PlaySFX(AudioClip clip, bool deleteAll)
    {
        AudioSource src = new GameObject("CLIP").AddComponent<AudioSource>();
        src.transform.SetParent(transform);
        src.transform.localPosition = Vector3.zero;
        src.transform.localRotation = Quaternion.identity;
        src.transform.localScale = Vector3.one;
        src.clip = clip;
        src.Play();
        if (deleteAll)
        {
            Invoke("DestroyAll", clip.length);
        }
    }
    // Destroy
    private void DestroyAll()
    {
        for (int c = 0; c < transform.childCount; c++)
        {
            DestroyImmediate(transform.GetChild(c).gameObject);
        }
    }

    // Play
    public static void PlaySFX(AudioClip clip)
    {
        MusicManager manager = GameObject.FindObjectOfType<MusicManager>();
        manager.PlaySFX(clip, false);
    }
}
