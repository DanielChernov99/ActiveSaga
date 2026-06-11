using UnityEngine;
using UnityEngine.Audio;

public class ActiveSagaAudioManager : MonoBehaviour
{
    public static ActiveSagaAudioManager Instance { get; private set; }

    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixerGroup uiMixerGroup;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource uiSource;

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
    }

    private void SetupSources()
    {
        if (musicSource == null)
        {
            musicSource = CreateAudioSource("Music Source", musicMixerGroup, true);
        }

        if (sfxSource == null)
        {
            sfxSource = CreateAudioSource("SFX Source", sfxMixerGroup, false);
        }

        if (uiSource == null)
        {
            uiSource = CreateAudioSource("UI Source", uiMixerGroup, false);
        }
    }

    private AudioSource CreateAudioSource(string sourceName, AudioMixerGroup mixerGroup, bool loop)
    {
        GameObject sourceObject = new GameObject(sourceName);
        sourceObject.transform.SetParent(transform);

        AudioSource source = sourceObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
        source.outputAudioMixerGroup = mixerGroup;

        return source;
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
}