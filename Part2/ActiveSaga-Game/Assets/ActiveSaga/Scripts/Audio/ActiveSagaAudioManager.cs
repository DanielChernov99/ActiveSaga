using UnityEngine;
using UnityEngine.Audio;

public class ActiveSagaAudioManager : MonoBehaviour
{
    public static ActiveSagaAudioManager Instance { get; private set; }

    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixerGroup uiMixerGroup;
    [SerializeField] private AudioMixerGroup voiceMixerGroup;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioSource voiceSource;

    [Header("Default Volumes")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.35f;

    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 0.8f;

    [Range(0f, 1f)]
    [SerializeField] private float uiVolume = 0.7f;

    [Range(0f, 1f)]
    [SerializeField] private float voiceVolume = 0.9f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupSources();
        ApplyDefaultVolumes();
    }

    private void SetupSources()
    {
        if (musicSource == null)
        {
            musicSource = CreateAudioSource("Music Source", musicMixerGroup, true, 0f);
        }

        if (sfxSource == null)
        {
            sfxSource = CreateAudioSource("SFX Source", sfxMixerGroup, false, 0f);
        }

        if (uiSource == null)
        {
            uiSource = CreateAudioSource("UI Source", uiMixerGroup, false, 0f);
        }

        if (voiceSource == null)
        {
            voiceSource = CreateAudioSource("Voice Source", voiceMixerGroup, false, 0f);
        }
    }

    private AudioSource CreateAudioSource(
        string sourceName,
        AudioMixerGroup mixerGroup,
        bool loop,
        float spatialBlend
    )
    {
        GameObject sourceObject = new GameObject(sourceName);
        sourceObject.transform.SetParent(transform);

        AudioSource source = sourceObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = spatialBlend;
        source.outputAudioMixerGroup = mixerGroup;

        return source;
    }

    private void ApplyDefaultVolumes()
    {
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
        SetUIVolume(uiVolume);
        SetVoiceVolume(voiceVolume);
    }

    public void PlayMusic(AudioClip musicClip)
    {
        if (musicClip == null)
        {
            return;
        }

        if (musicSource.clip == musicClip && musicSource.isPlaying)
        {
            return;
        }

        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource == null)
        {
            return;
        }

        musicSource.Stop();
    }

    public void PlaySFX(AudioClip sfxClip)
    {
        if (sfxClip == null)
        {
            return;
        }

        sfxSource.PlayOneShot(sfxClip);
    }

    public void PlayUI(AudioClip uiClip)
    {
        if (uiClip == null)
        {
            return;
        }

        uiSource.PlayOneShot(uiClip);
    }

    public void PlayVoice(AudioClip voiceClip)
    {
        if (voiceClip == null)
        {
            return;
        }

        voiceSource.PlayOneShot(voiceClip);
    }

    public void PlayRandomVoice(AudioClip[] voiceClips)
    {
        if (voiceClips == null || voiceClips.Length == 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, voiceClips.Length);
        PlayVoice(voiceClips[randomIndex]);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);

        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);

        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    public void SetUIVolume(float volume)
    {
        uiVolume = Mathf.Clamp01(volume);

        if (uiSource != null)
        {
            uiSource.volume = uiVolume;
        }
    }

    public void SetVoiceVolume(float volume)
    {
        voiceVolume = Mathf.Clamp01(volume);

        if (voiceSource != null)
        {
            voiceSource.volume = voiceVolume;
        }
    }
}