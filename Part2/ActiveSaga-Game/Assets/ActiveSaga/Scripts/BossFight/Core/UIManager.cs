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

        private void OnEnable()
        {
            EventManager.Subscribe<HealthChangedEvent>(OnHealthChanged);
            EventManager.Subscribe<WaveStartedEvent>(OnWaveStarted);
        }

        private void OnDisable()
        {
            EventManager.Unsubscribe<HealthChangedEvent>(OnHealthChanged);
            EventManager.Unsubscribe<WaveStartedEvent>(OnWaveStarted);
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
    }
}