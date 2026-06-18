using UnityEngine;

/// <summary>
/// Менеджер звуков. Сохраняется между сценами (DontDestroyOnLoad).
/// Управляет звуковыми эффектами (нахождение предмета, промах) и музыкой.
/// </summary>
public class SoundManager : PersistentManager<SoundManager>
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource; // Для звуков предметов
    [SerializeField] private AudioSource musicSource; // Для музыки (опционально)

    [Header("Sound Effects")]
    public AudioClip missSound; // Звук при клике мимо искомых объектов
    public AudioClip foundSound; // Звук при нахождении нужного объекта

    [Header("Audio Settings")]
    [Range(0, 1)] public float sfxVolume = 1f;
    [Range(0, 1)] public float musicVolume = 1f;

    protected override void OnInit()
    {
        // Создаем AudioSources, если они не назначены в инспекторе
        if (sfxSource == null)
        {
            GameObject sfxGO = new GameObject("SFX Source");
            sfxGO.transform.SetParent(transform);
            sfxSource = sfxGO.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        if (musicSource == null)
        {
            GameObject musicGO = new GameObject("Music Source");
            musicGO.transform.SetParent(transform);
            musicSource = musicGO.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
        }
    }

    /// <summary>
    /// Проигрывает звук при клике мимо искомых объектов
    /// </summary>
    public void PlayMissSound()
    {
        if (missSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(missSound, sfxVolume);
        }
    }

    /// <summary>
    /// Проигрывает звук при нахождении нужного объекта
    /// </summary>
    public void PlayFoundSound()
    {
        if (foundSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(foundSound, sfxVolume);
        }
    }

    /// <summary>
    /// Проигрывает музыку
    /// </summary>
    public void PlayMusic(AudioClip musicClip)
    {
        if (musicClip != null && musicSource != null)
        {
            musicSource.clip = musicClip;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }

    /// <summary>
    /// Останавливает музыку
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    /// <summary>
    /// Устанавливает громкость SFX
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    /// <summary>
    /// Устанавливает громкость музыки
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }
}