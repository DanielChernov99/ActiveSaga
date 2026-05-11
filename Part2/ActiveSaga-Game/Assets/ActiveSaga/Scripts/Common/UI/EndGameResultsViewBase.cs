using TMPro;
using UnityEngine;
using ActiveSaga.Common.GameSession;
using ActiveSaga.Common.Networking;

namespace ActiveSaga.Common.UI
{
    public abstract class EndGameResultsViewBase : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private GameObject errorPanel;

        [Header("Result Texts")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI durationText;
        [SerializeField] private TextMeshProUGUI xpText;
        [SerializeField] private TextMeshProUGUI moneyText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI gameStatsText;

        [Header("Error Text")]
        [SerializeField] private TextMeshProUGUI errorText;

        private void Start()
        {
            HideAll();
        }

        public void ShowLoading()
        {
            SetActive(rootPanel, true);
            SetActive(loadingPanel, true);
            SetActive(resultsPanel, false);
            SetActive(errorPanel, false);
        }

        public void ShowResults(
            ServerGameResultResponse response,
            GameEndReason endReason,
            float durationSeconds,
            GameStatsSnapshot statsSnapshot)
        {
            if (response == null)
            {
                ShowError("No server response.");
                return;
            }

            SetActive(rootPanel, true);
            SetActive(loadingPanel, false);
            SetActive(resultsPanel, true);
            SetActive(errorPanel, false);

            SetText(titleText, BuildTitle(endReason));
            SetText(durationText, "Time Played: " + FormatDuration(durationSeconds));
            SetText(xpText, BuildXpText(response));
            SetText(moneyText, BuildMoneyText(response));
            SetText(levelText, BuildLevelText(response));
            SetText(gameStatsText, BuildGameSpecificStatsText(statsSnapshot));
        }

        public void ShowError(string message)
        {
            SetActive(rootPanel, true);
            SetActive(loadingPanel, false);
            SetActive(resultsPanel, false);
            SetActive(errorPanel, true);

            SetText(errorText, message);
        }

        public void HideAll()
        {
            SetActive(rootPanel, false);
            SetActive(loadingPanel, false);
            SetActive(resultsPanel, false);
            SetActive(errorPanel, false);
        }

        protected abstract string BuildGameSpecificStatsText(GameStatsSnapshot statsSnapshot);

        private string BuildTitle(GameEndReason endReason)
        {
            switch (endReason)
            {
                case GameEndReason.GameWon:
                    return "Game Won";

                case GameEndReason.PlayerQuit:
                    return "Game Ended";

                case GameEndReason.GameOver:
                    return "Game Over";

                default:
                    return "Game Results";
            }
        }

        private string BuildXpText(ServerGameResultResponse response)
        {
            if (response.rewards == null)
            {
                return "XP Earned: 0";
            }

            return "XP Earned: " + response.rewards.totalXp;
        }

        private string BuildMoneyText(ServerGameResultResponse response)
        {
            if (response.rewards == null)
            {
                return "Money Earned: 0";
            }

            return "Money Earned: " + response.rewards.totalMoney;
        }

        private string BuildLevelText(ServerGameResultResponse response)
        {
            if (response.player == null)
            {
                return "Player progression missing.";
            }

            string levelLine;

            if (response.leveledUp)
            {
                levelLine = "Level Up! " + response.previousLevel + " -> " + response.player.level;
            }
            else
            {
                levelLine = "No Level Up";
            }

            return
                levelLine +
                "\nCurrent Level: " + response.player.level +
                "\nXP: " + response.player.currentXp + " / " + response.player.xpNeededForNextLevel;
        }

        private string FormatDuration(float totalSeconds)
        {
            int seconds = Mathf.FloorToInt(totalSeconds);
            int minutes = seconds / 60;
            int remainingSeconds = seconds % 60;

            return minutes + "m " + remainingSeconds + "s";
        }

        private void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

        private void SetText(TextMeshProUGUI text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }
    }
}