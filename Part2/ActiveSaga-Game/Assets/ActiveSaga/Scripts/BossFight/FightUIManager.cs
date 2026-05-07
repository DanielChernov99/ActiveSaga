using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ActiveSaga.BossFight
{
    public class FightUIManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BossFightManager bossFightManager;
        
        [Header("HP UI")]
        [SerializeField] private Slider bossHPBar;
        [SerializeField] private Slider playerHPBar;
        
        [Header("Text UI")]
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI feedbackText;

        private void Start()
        {
            if (bossFightManager == null)
                bossFightManager = FindFirstObjectByType<BossFightManager>();

            if (bossHPBar != null) bossHPBar.maxValue = bossFightManager.BossHP;
            if (playerHPBar != null) playerHPBar.maxValue = bossFightManager.PlayerHP;
            
            if (feedbackText != null) feedbackText.text = "";
        }

        private void Update()
        {
            if (bossFightManager == null) return;

            // Update HP Bars
            if (bossHPBar != null) bossHPBar.value = bossFightManager.BossHP;
            if (playerHPBar != null) playerHPBar.value = bossFightManager.PlayerHP;

            // Update Wave Info
            if (waveText != null)
            {
                // We might need to expose wave number in BossFightManager or calculate it from difficulty
                float currentWaveNum = (bossFightManager.DifficultyMultiplier - 1.0f) * 10 + 1;
                waveText.text = $"Wave: {Mathf.RoundToInt(currentWaveNum)}";
            }
        }

        public void ShowFeedback(string message, float duration = 2f)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message;
                CancelInvoke(nameof(ClearFeedback));
                Invoke(nameof(ClearFeedback), duration);
            }
        }

        private void ClearFeedback()
        {
            if (feedbackText != null) feedbackText.text = "";
        }
    }
}
