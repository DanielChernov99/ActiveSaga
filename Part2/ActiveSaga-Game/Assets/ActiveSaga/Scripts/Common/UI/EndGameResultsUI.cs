using System.Text;
using TMPro;
using UnityEngine;
using ActiveSaga.Common.Networking;

namespace ActiveSaga.Common.UI
{
    public class EndGameResultsUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject rootPanel;
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private GameObject errorPanel;

        [Header("Result Texts")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI xpText;
        [SerializeField] private TextMeshProUGUI moneyText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI missionsText;

        [Header("Error Text")]
        [SerializeField] private TextMeshProUGUI errorText;

        private void Start()
        {
            HideAll();
        }

        public void ShowLoading()
        {
            if (rootPanel != null) rootPanel.SetActive(true);
            if (loadingPanel != null) loadingPanel.SetActive(true);
            if (resultsPanel != null) resultsPanel.SetActive(false);
            if (errorPanel != null) errorPanel.SetActive(false);
        }

        public void ShowResults(ServerGameResultResponse response)
        {
            if (rootPanel != null) rootPanel.SetActive(true);
            if (loadingPanel != null) loadingPanel.SetActive(false);
            if (resultsPanel != null) resultsPanel.SetActive(true);
            if (errorPanel != null) errorPanel.SetActive(false);

            if (response == null)
            {
                ShowError("No server response.");
                return;
            }

            if (titleText != null)
            {
                titleText.text = "Game Results";
            }

            if (xpText != null)
            {
                if (response.rewards != null)
                {
                    xpText.text =
                        "Gameplay XP: " + response.rewards.gameplayXp +
                        "\nMission Bonus XP: " + response.rewards.missionBonusXp +
                        "\nTotal XP: " + response.rewards.totalXp;
                }
                else
                {
                    xpText.text = "XP: 0";
                }
            }

            if (moneyText != null)
            {
                if (response.rewards != null)
                {
                    moneyText.text =
                        "Gameplay Money: " + response.rewards.gameplayMoney +
                        "\nMission Bonus Money: " + response.rewards.missionBonusMoney +
                        "\nTotal Money: " + response.rewards.totalMoney;
                }
                else
                {
                    moneyText.text = "Money: 0";
                }
            }

            if (levelText != null)
            {
                if (response.player != null)
                {
                    levelText.text =
                        "Level: " + response.player.level +
                        "\nXP: " + response.player.currentXp + " / " + response.player.xpNeededForNextLevel +
                        "\nMoney: " + response.player.money;
                }
                else
                {
                    levelText.text = "Player data missing.";
                }
            }

            if (missionsText != null)
            {
                missionsText.text = BuildMissionText(response);
            }
        }

        public void ShowError(string message)
        {
            if (rootPanel != null) rootPanel.SetActive(true);
            if (loadingPanel != null) loadingPanel.SetActive(false);
            if (resultsPanel != null) resultsPanel.SetActive(false);
            if (errorPanel != null) errorPanel.SetActive(true);

            if (errorText != null)
            {
                errorText.text = message;
            }
        }

        public void HideAll()
        {
            if (rootPanel != null) rootPanel.SetActive(false);
            if (loadingPanel != null) loadingPanel.SetActive(false);
            if (resultsPanel != null) resultsPanel.SetActive(false);
            if (errorPanel != null) errorPanel.SetActive(false);
        }

        private string BuildMissionText(ServerGameResultResponse response)
        {
            if (response == null || response.missionReport == null || response.missionReport.Length == 0)
            {
                return "No missions completed.";
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Missions:");

            foreach (ServerMissionReportItem mission in response.missionReport)
            {
                if (mission == null)
                {
                    continue;
                }

                if (mission.completed)
                {
                    sb.AppendLine(
                        mission.title +
                        " +" + mission.rewardXp + " XP" +
                        " +" + mission.rewardMoney + " Money"
                    );
                }
                else
                {
                    sb.AppendLine(
                        mission.title +
                        " " + mission.currentValue +
                        " / " + mission.targetValue
                    );
                }
            }

            return sb.ToString();
        }
    }
}