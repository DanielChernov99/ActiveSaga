using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    [Header("Optional Override")]
    [SerializeField] private AudioClip customClickClip;

    private Button button;
    private bool listenerRegistered;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        RegisterListener();
    }

    private void OnDisable()
    {
        UnregisterListener();
    }

    private void RegisterListener()
    {
        if (listenerRegistered)
        {
            return;
        }

        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button == null)
        {
            return;
        }

        button.onClick.AddListener(PlayClickSound);
        listenerRegistered = true;
    }

    private void UnregisterListener()
    {
        if (!listenerRegistered)
        {
            return;
        }

        if (button != null)
        {
            button.onClick.RemoveListener(PlayClickSound);
        }

        listenerRegistered = false;
    }

    private void PlayClickSound()
    {
        if (ActiveSagaAudioManager.Instance == null)
        {
            Debug.LogWarning("UIButtonSound: ActiveSagaAudioManager was not found.");
            return;
        }

        if (customClickClip != null)
        {
            ActiveSagaAudioManager.Instance.PlayUI(customClickClip);
            return;
        }

        ActiveSagaAudioManager.Instance.PlayDefaultClick();
    }
}