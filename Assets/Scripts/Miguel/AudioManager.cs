/*using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Centralized audio management system for music and sound effects.
/// Provides singleton access for consistent audio playback throughout the game.
/// Handles background music transitions, sound effect playback, and volume controls.
/// Integrates with game events for responsive audio feedback.
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource; // Dedicated source for background music (looping)
    public AudioSource sfxSource; // Dedicated source for sound effects (one-shot)

    [Header("Music Clips")]
    public AudioClip menuMusic; // Background music for main menu
    public AudioClip gameMusic; // Background music during adventure gameplay
    public AudioClip gameOverMusic; // Background music for game over screen

    [Header("SFX Clips")]
    public AudioClip jumpSound; // Player jump sound effect
    public AudioClip attackSound; // Player sword attack sound effect
    public AudioClip enemyHitSound; // Enemy taking damage sound effect
    public AudioClip enemyDeathSound; // Enemy death/defeat sound effect
    public AudioClip powerupSound; // Powerup collection sound effect
    public AudioClip damageSound; // Player taking damage sound effect
    public AudioClip victorySound; // Victory/win condition sound effect

    [Header("Settings")]
    public float musicVolume = 0.7f; // Master volume for background music (0-1)
    public float sfxVolume = 0.8f; // Master volume for sound effects (0-1)

    // Singleton pattern for global access
    public static AudioManager Instance { get; private set; }

    // Internal sound library for easy access by name
    private Dictionary<string, AudioClip> soundLibrary;

    /// <summary>
    /// Initialize singleton pattern and set up audio system
    /// Ensures only one AudioManager exists across scene loads
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scene changes
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    /// <summary>
    /// Set up audio sources and build sound library for easy access
    /// Creates dedicated audio sources for music and SFX if not assigned
    /// </summary>
    private void InitializeAudio()
    {
        if (musicSource == null)
        {
            GameObject musicGO = new GameObject("MusicSource");
            musicGO.transform.SetParent(transform);
            musicSource = musicGO.AddComponent<AudioSource>();
            musicSource.loop = true; // Music should loop continuously
            musicSource.volume = musicVolume;
        }

        if (sfxSource == null)
        {
            GameObject sfxGO = new GameObject("SFXSource");
            sfxGO.transform.SetParent(transform);
            sfxSource = sfxGO.AddComponent<AudioSource>();
            sfxSource.volume = sfxVolume;
        }

        soundLibrary = new Dictionary<string, AudioClip>
        {
            { "jump", jumpSound },
            { "attack", attackSound },
            { "enemy_hit", enemyHitSound },
            { "enemy_death", enemyDeathSound },
            { "powerup", powerupSound },
            { "damage", damageSound },
            { "victory", victorySound }
        };
    }

    /// <summary>
    /// Play a sound effect by name from the sound library
    /// </summary>
    public void PlaySFX(string soundName)
    {
        if (soundLibrary.ContainsKey(soundName) && soundLibrary[soundName] != null)
        {
            sfxSource.PlayOneShot(soundLibrary[soundName]);
        }
        else
        {
            Debug.LogWarning($"Sound '{soundName}' not found in library!");
        }
    }

    /// <summary>
    /// Play a one-shot sound effect directly from a provided AudioClip
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Play background music using a specific AudioClip
    /// </summary>
    public void PlayMusic(AudioClip musicClip)
    {
        if (musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.Play();
        }
    }

    /// <summary>
    /// Play background music by type name (e.g., "menu", "game", "gameover")
    /// </summary>
    public void PlayMusic(string musicType)
    {
        AudioClip clipToPlay = null;

        switch (musicType.ToLower())
        {
            case "menu":
                clipToPlay = menuMusic;
                break;
            case "game":
                clipToPlay = gameMusic;
                break;
            case "gameover":
                clipToPlay = gameOverMusic;
                break;
        }

        if (clipToPlay != null && musicSource.clip != clipToPlay)
        {
            musicSource.clip = clipToPlay;
            musicSource.Play();
        }
    }

    /// <summary>
    /// Switch music based on GameManager phase
    /// </summary>
    public void SetGamePhaseMusic(GameManager.GamePhase phase)
    {
        switch (phase)
        {
            case GameManager.GamePhase.Menu:
                PlayMusic(menuMusic);
                break;
            case GameManager.GamePhase.Adventure:
                PlayMusic(gameMusic);
                break;
            case GameManager.GamePhase.GameOver:
                PlayMusic(gameOverMusic);
                break;
        }
    }

    /// <summary>
    /// Adjust music volume at runtime
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    /// <summary>
    /// Adjust sound effect volume at runtime
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }

    /// <summary>
    /// Stop current music track
    /// </summary>
    public void StopMusic() => musicSource.Stop();

    /// <summary>
    /// Pause current music
    /// </summary>
    public void PauseMusic() => musicSource.Pause();

    /// <summary>
    /// Resume paused music
    /// </summary>
    public void ResumeMusic() => musicSource.UnPause();

    /// <summary>
    /// Play SFX with a slight pitch variation to add audio variety
    /// </summary>
    public void PlayRandomPitchSFX(string soundName, float pitchVariation = 0.1f)
    {
        if (soundLibrary.ContainsKey(soundName) && soundLibrary[soundName] != null)
        {
            float originalPitch = sfxSource.pitch;
            sfxSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            sfxSource.PlayOneShot(soundLibrary[soundName]);
            sfxSource.pitch = originalPitch;
        }
    }

    // ---------- Game Event Wrappers ----------

    public void OnPlayerJump() => PlaySFX("jump");
    public void OnPlayerAttack() => PlaySFX("attack");
    public void OnPlayerDamage() => PlaySFX("damage");
    public void OnEnemyHit() => PlayRandomPitchSFX("enemy_hit", 0.2f);
    public void OnEnemyDeath() => PlaySFX("enemy_death");
    public void OnPowerupCollected() => PlaySFX("powerup");
    public void OnVictory() => PlaySFX("victory");

    public void OnGameStart() => PlayMusic("game");

    public void OnGameOver(bool victory)
    {
        if (victory) PlaySFX("victory");
        PlayMusic("gameover");
    }

    public void OnMenuOpen() => PlayMusic("menu");
}*/
