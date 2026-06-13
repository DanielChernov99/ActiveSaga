using UnityEngine;
using UnityEngine.Audio;

public class Looping3DAudioEmitter : MonoBehaviour
{
    [Header("Loop Clip")]
    [SerializeField] private AudioClip loopClip;
    [SerializeField] private bool playOnEnable = true;

    [Header("Mixer")]
    [SerializeField] private AudioMixerGroup outputMixerGroup;

    [Header("3D Audio")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.7f;

    [Range(0f, 1f)]
    [SerializeField] private float spatialBlend = 1f;

    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float maxDistance = 35f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        ApplySettings();
    }

    private void OnEnable()
    {
        if (playOnEnable)
        {
            PlayLoop();
        }
    }

    private void OnDisable()
    {
        StopLoop();
    }

    private void OnValidate()
    {
        if (maxDistance < minDistance)
        {
            maxDistance = minDistance;
        }

        if (audioSource != null)
        {
            ApplySettings();
        }
    }

    public void PlayLoop()
    {
        if (loopClip == null)
        {
            Debug.LogWarning("Looping3DAudioEmitter: Loop clip is missing on " + gameObject.name);
            return;
        }

        if (audioSource == null)
        {
            return;
        }

        if (audioSource.isPlaying)
        {
            return;
        }

        audioSource.clip = loopClip;
        audioSource.Play();
    }

    public void StopLoop()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    private void ApplySettings()
    {
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = spatialBlend;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.volume = volume;
        audioSource.outputAudioMixerGroup = outputMixerGroup;
    }
}