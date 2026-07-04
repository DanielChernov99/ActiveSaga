using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ActiveSaga.BossFight.Core
{
    public class UIManager : MonoBehaviour
    {
        [Header("HP UI")]
        [SerializeField] private Slider playerHPBar;
        
        [Header("Text UI")]
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI feedbackText;

        private Coroutine feedbackRoutine;

        private void Awake()
        {
            if (feedbackText != null)
            {
                feedbackText.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            EventManager.Subscribe<HealthChangedEvent>(OnHealthChanged);
            EventManager.Subscribe<WaveStartedEvent>(OnWaveStarted);
            EventManager.Subscribe<FeedbackEvent>(OnFeedback);
        }

        private void OnDisable()
        {
            EventManager.Unsubscribe<HealthChangedEvent>(OnHealthChanged);
            EventManager.Unsubscribe<WaveStartedEvent>(OnWaveStarted);
            EventManager.Unsubscribe<FeedbackEvent>(OnFeedback);
        }

        private void OnHealthChanged(HealthChangedEvent e)
        {
            // Update UI only if the health change belongs to the player
            if (e.isPlayer)
            {
                if (playerHPBar != null)
                {
                    playerHPBar.maxValue = e.max;
                    playerHPBar.value = e.current;
                }
            }
        }

        private void OnWaveStarted(WaveStartedEvent e)
        {
            // Update the text component to display the current wave number
            if (waveText != null)
            {
                waveText.text = $"Wave: {e.waveIndex}";
            }
        }

        private void OnFeedback(FeedbackEvent e)
        {
            if (feedbackText == null)
            {
                return;
            }

            if (feedbackRoutine != null)
            {
                StopCoroutine(feedbackRoutine);
            }

            feedbackRoutine = StartCoroutine(ShowFeedbackRoutine(e.message, e.duration));
        }

        private IEnumerator ShowFeedbackRoutine(string message, float duration)
        {
            feedbackText.text = message;
            feedbackText.gameObject.SetActive(true);

            yield return new WaitForSeconds(Mathf.Max(0.1f, duration));

            feedbackText.gameObject.SetActive(false);
            feedbackRoutine = null;
        }
    }
}
