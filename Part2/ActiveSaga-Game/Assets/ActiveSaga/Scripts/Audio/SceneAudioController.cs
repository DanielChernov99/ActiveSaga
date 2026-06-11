using UnityEngine;

public class SceneAudioController : MonoBehaviour
{
    [Header("Scene Music")]
    [SerializeField] private AudioClip sceneMusic;
    [SerializeField] private bool playMusicOnStart = true;

    private void Start()
    {
        if (!playMusicOnStart)
        {
            return;
        }

        if (ActiveSagaAudioManager.Instance == null)
        {
            Debug.LogWarning("SceneAudioController: ActiveSagaAudioManager was not found.");
            return;
        }

        ActiveSagaAudioManager.Instance.PlayMusic(sceneMusic);
    }
}