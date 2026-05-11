using UnityEngine;
using UnityEngine.UI;
using ActiveSaga.BossFight.Core;

namespace ActiveSaga.BossFight.UI
{
    public class BossUI : MonoBehaviour
    {
        [SerializeField] private Slider bossHealthBar;

        private void OnEnable()
        {
            EventManager.Subscribe<HealthChangedEvent>(OnHealthChanged);
        }

        private void OnDisable()
        {
            EventManager.Unsubscribe<HealthChangedEvent>(OnHealthChanged);
        }

        private void OnHealthChanged(HealthChangedEvent e)
        {
            // Ignore player HP updates
            if (e.isPlayer) return;

            // Normalized value (current / max) - assumes slider is set to 0-1
            float value = (e.max > 0f) ? (e.current / e.max) : 0f;
            
            if (bossHealthBar != null)
            {
                bossHealthBar.value = value;
            }
        }
    }
}
