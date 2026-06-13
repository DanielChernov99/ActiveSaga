using UnityEngine;
using UnityEngine.Audio;

public class Random3DAudioEmitter : MonoBehaviour
{
    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] roarClips;
    [SerializeField] private AudioClip[] lineClips;

    [Header("Automatic Playback")]
    [SerializeField] private bool playAutomatically = true;
    [SerializeField] private bool preventOverlappingSounds = true;

    [Header("First Sound Delay")]
    [SerializeField] private float firstSoundMinDelay = 2f;
    [SerializeField] private float firstSoundMaxDelay = 5f;

    [Header("Delay Between Sounds")]
    [SerializeField] private float minDelayBetweenSounds = 4f;
    [SerializeField] private float maxDelayBetweenSounds = 9f;

    [Header("Random Choice")]
    [Range(0f, 1f)]
    [SerializeField] private float roarChance = 0.6f;

    [Header("Optional Animation - Fight Game Boss")]
    [SerializeField] private bool playAnimationsWithSounds = false;
    [SerializeField] private Animator animator;
    [SerializeField] private string roarTriggerName = "Roar";
    [SerializeField] private string lineTriggerName = "Talk";

    [Header("3D Audio")]
    [SerializeField] private AudioMixerGroup outputMixerGroup;

    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float spatialBlend = 1f;

    [SerializeField] private float minDistance = 8f;
    [SerializeField] private float maxDistance = 75f;

    private AudioSource audioSource;
    private float nextPlayTime;

    private enum SoundType
    {
        Roar,
        Line
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        ApplyAudioSourceSettings();
    }

    private void OnEnable()
    {
        ScheduleFirstSound();
    }

    private void OnValidate()
    {
        if (firstSoundMaxDelay < firstSoundMinDelay)
        {
            firstSoundMaxDelay = firstSoundMinDelay;
        }

        if (maxDelayBetweenSounds < minDelayBetweenSounds)
        {
            maxDelayBetweenSounds = minDelayBetweenSounds;
        }

        if (maxDistance < minDistance)
        {
            maxDistance = minDistance;
        }
    }

    private void Update()
    {
        if (!playAutomatically)
        {
            return;
        }

        if (Time.time < nextPlayTime)
        {
            return;
        }

        if (preventOverlappingSounds && audioSource.isPlaying)
        {
            return;
        }

        PlayRandomSound();
    }

    public void PlayRandomSound()
    {
        SoundType soundType;
        AudioClip clip = PickRandomClip(out soundType);

        if (clip == null)
        {
            ScheduleNextSound(0f);
            return;
        }

        PlayAnimationForSound(soundType);

        audioSource.PlayOneShot(clip);
        ScheduleNextSound(preventOverlappingSounds ? clip.length : 0f);
    }

    private void PlayAnimationForSound(SoundType soundType)
    {
        if (!playAnimationsWithSounds || animator == null)
        {
            return;
        }

        if (soundType == SoundType.Roar)
        {
            SetTriggerIfValid(roarTriggerName);
            return;
        }

        SetTriggerIfValid(lineTriggerName);
    }

    private void SetTriggerIfValid(string triggerName)
    {
        if (string.IsNullOrWhiteSpace(triggerName))
        {
            return;
        }

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.type == AnimatorControllerParameterType.Trigger &&
                parameter.name == triggerName)
            {
                animator.SetTrigger(triggerName);
                return;
            }
        }

        Debug.LogWarning("Random3DAudioEmitter: Animator trigger was not found: " + triggerName);
    }

    private void ApplyAudioSourceSettings()
    {
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = spatialBlend;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.volume = volume;
        audioSource.outputAudioMixerGroup = outputMixerGroup;
    }

    private AudioClip PickRandomClip(out SoundType soundType)
    {
        bool shouldRoar = Random.value <= roarChance;

        if (shouldRoar && HasClips(roarClips))
        {
            soundType = SoundType.Roar;
            return roarClips[Random.Range(0, roarClips.Length)];
        }

        if (HasClips(lineClips))
        {
            soundType = SoundType.Line;
            return lineClips[Random.Range(0, lineClips.Length)];
        }

        if (HasClips(roarClips))
        {
            soundType = SoundType.Roar;
            return roarClips[Random.Range(0, roarClips.Length)];
        }

        soundType = SoundType.Line;
        return null;
    }

    private bool HasClips(AudioClip[] clips)
    {
        return clips != null && clips.Length > 0;
    }

    private void ScheduleFirstSound()
    {
        nextPlayTime = Time.time + Random.Range(firstSoundMinDelay, firstSoundMaxDelay);
    }

    private void ScheduleNextSound(float extraDelay)
    {
        nextPlayTime = Time.time + extraDelay + Random.Range(minDelayBetweenSounds, maxDelayBetweenSounds);
    }
}