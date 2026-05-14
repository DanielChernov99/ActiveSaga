using TMPro;
using UnityEngine;
using ActiveSaga.Common.GameSession;

namespace ActiveSaga.Common.UI
{
    public class GameTimerUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private string prefix = "TIME: ";

        private void Awake()
        {
            if (timerText == null)
            {
                timerText = GetComponent<TextMeshProUGUI>();
            }
        }

        private void Update()
        {
            if (timerText == null)
            {
                return;
            }

            if (GameSessionManager.Instance == null)
            {
                timerText.text = prefix + "00:00";
                return;
            }

            float activeSeconds = GameSessionManager.Instance.GetActiveDurationSeconds();
            timerText.text = prefix + FormatTime(activeSeconds);
        }

        private string FormatTime(float totalSeconds)
        {
            int seconds = Mathf.FloorToInt(totalSeconds);
            int minutes = seconds / 60;
            int remainingSeconds = seconds % 60;

            return minutes.ToString("00") + ":" + remainingSeconds.ToString("00");
        }
    }
}