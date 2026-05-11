using TMPro;
using UnityEngine;
using UnityEngine.UI;
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

        [Header("Title")]
        [SerializeField] private TextMeshProUGUI titleText;

        [Header("Top Summary Values")]
        [SerializeField] private TextMeshProUGUI timeValueText;
        [SerializeField] private TextMeshProUGUI xpEarnedValueText;
        [SerializeField] private TextMeshProUGUI moneyEarnedValueText;

        [Header("Level Section")]
        [SerializeField] private TextMeshProUGUI levelUpText;
        [SerializeField] private TextMeshProUGUI currentLevelText;
        [SerializeField] private TextMeshProUGUI xpProgressText;
        [SerializeField] private Image xpBarFill;

        [Header("Game Stat 1")]
        [SerializeField] private TextMeshProUGUI stat1LabelText;
        [SerializeField] private TextMeshProUGUI stat1ValueText;

        [Header("Game Stat 2")]
        [SerializeField] private TextMeshProUGUI stat2LabelText;
        [SerializeField] private TextMeshProUGUI stat2ValueText;

        [Header("Game Stat 3")]
        [SerializeField] private TextMeshProUGUI stat3LabelText;
        [SerializeField] private TextMeshProUGUI stat3ValueText;

        [Header("Error Text")]
        [SerializeField] private TextMeshProUGUI errorText;

        protected struct ResultStat
        {
            public string label;
            public string value;

            public ResultStat(string label, string value)
            {
                this.label = label;
                this.value = value;
            }
        }

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

            SetText(timeValueText, FormatDuration(durationSeconds));
            SetText(xpEarnedValueText, GetTotalXp(response).ToString());
            SetText(moneyEarnedValueText, GetTotalMoney(response).ToString());

            UpdateLevelSection(response);
            UpdateGameSpecificStats(statsSnapshot);
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

        protected abstract void BuildGameSpecificStats(
            GameStatsSnapshot statsSnapshot,
            out ResultStat stat1,
            out ResultStat stat2,
            out ResultStat stat3
        );

        private void UpdateLevelSection(ServerGameResultResponse response)
        {
            if (response.player == null)
            {
                SetText(levelUpText, "PROGRESSION MISSING");
                SetText(currentLevelText, "CURRENT LEVEL: -");
                SetText(xpProgressText, "XP: - / -");

                if (xpBarFill != null)
                {
                    xpBarFill.fillAmount = 0f;
                }

                return;
            }

            if (response.leveledUp)
            {
                SetText(levelUpText, "LEVEL UP! " + response.previousLevel + " -> " + response.player.level);
            }
            else
            {
                SetText(levelUpText, "NO LEVEL UP");
            }

            SetText(currentLevelText, "CURRENT LEVEL: " + response.player.level);
            SetText(xpProgressText, "XP: " + response.player.currentXp + " / " + response.player.xpNeededForNextLevel);

            if (xpBarFill != null)
            {
                float fillAmount = 0f;

                if (response.player.xpNeededForNextLevel > 0)
                {
                    fillAmount = Mathf.Clamp01(
                        (float)response.player.currentXp / response.player.xpNeededForNextLevel
                    );
                }

                xpBarFill.fillAmount = fillAmount;
            }
        }

        private void UpdateGameSpecificStats(GameStatsSnapshot statsSnapshot)
        {
            BuildGameSpecificStats(statsSnapshot, out ResultStat stat1, out ResultStat stat2, out ResultStat stat3);

            SetText(stat1LabelText, stat1.label);
            SetText(stat1ValueText, stat1.value);

            SetText(stat2LabelText, stat2.label);
            SetText(stat2ValueText, stat2.value);

            SetText(stat3LabelText, stat3.label);
            SetText(stat3ValueText, stat3.value);
        }

        private string BuildTitle(GameEndReason endReason)
        {
            switch (endReason)
            {
                case GameEndReason.GameWon:
                    return "GAME WON";

                case GameEndReason.PlayerQuit:
                    return "GAME ENDED";

                case GameEndReason.GameOver:
                    return "GAME OVER";

                default:
                    return "GAME RESULTS";
            }
        }

        private int GetTotalXp(ServerGameResultResponse response)
        {
            if (response.rewards == null)
            {
                return 0;
            }

            return response.rewards.totalXp;
        }

        private int GetTotalMoney(ServerGameResultResponse response)
        {
            if (response.rewards == null)
            {
                return 0;
            }

            return response.rewards.totalMoney;
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