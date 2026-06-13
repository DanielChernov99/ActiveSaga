using UnityEngine;

public class AudioEventOnEnable : MonoBehaviour
{
    public enum AudioEventType
    {
        SFX,
        UI,
        Voice
    }

    [Header("Audio")]
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private AudioEventType audioEventType = AudioEventType.SFX;

    [Header("Playback")]
    [SerializeField] private bool playOnlyOnce = true;

    private bool hasPlayed;

    private void OnEnable()
    {
        if (playOnlyOnce && hasPlayed)
        {
            return;
        }

        if (audioClip == null)
        {
            Debug.LogWarning("AudioEventOnEnable: Audio clip is missing on " + gameObject.name);
            return;
        }

        if (ActiveSagaAudioManager.Instance == null)
        {
            Debug.LogWarning("AudioEventOnEnable: ActiveSagaAudioManager was not found.");
            return;
        }

        PlayClip();

        hasPlayed = true;
    }

    private void PlayClip()
    {
        switch (audioEventType)
        {
            case AudioEventType.UI:
                ActiveSagaAudioManager.Instance.PlayUI(audioClip);
                break;

            case AudioEventType.Voice:
                ActiveSagaAudioManager.Instance.PlayVoice(audioClip);
                break;

            default:
                ActiveSagaAudioManager.Instance.PlaySFX(audioClip);
                break;
        }
    }
}