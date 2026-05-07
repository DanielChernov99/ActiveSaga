using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ActiveSaga.BossFight.Core
{
    public class UIManager : MonoBehaviour
    {
        [Header("HP UI")]
        [SerializeField] private Slider bossHPBar;
        [SerializeField] private Slider playerHPBar;
        
        [Header("Text UI")]
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI feedbackText;

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
            if (e.isPlayer)
            {
                if (playerHPBar != null)
                {
                    playerHPBar.maxValue = e.max;
                    playerHPBar.value = e.current;
                }
            }
            else
            {
                if (bossHPBar != null)
                {
                    bossHPBar.maxValue = e.max;
                    bossHPBar.value = e.current;
                }
            }
        }

        private void OnWaveStarted(WaveStartedEvent e)
        {
            if (waveText != null)
            {
                waveText.text = $"Wave: {e.waveIndex} - {e.name}";
            }
        }

        private void OnFeedback(FeedbackEvent e)
        {
            if (feedbackText != null)
            {
                feedbackText.text = e.message;
                CancelInvoke(nameof(ClearFeedback));
                Invoke(nameof(ClearFeedback), e.duration);
            }
        }

        private void ClearFeedback()
        {
            if (feedbackText != null) feedbackText.text = "";
        }
    }
}
