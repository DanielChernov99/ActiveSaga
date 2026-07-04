using ActiveSaga.MainScreen.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ActiveSaga.MainScreen.UI
{
    public class PlayerStatsScreenUI : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private DashboardDataManager dashboardDataManager;

        [Header("Text References")]
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text xpText;
        [SerializeField] private TMP_Text totalActiveTimeText;
        [SerializeField] private TMP_Text totalDistanceText;
        [SerializeField] private TMP_Text totalJumpsText;

        [Header("XP Bar")]
        [SerializeField] private Image xpFillImage;

        [Header("Debug")]
        [SerializeField] private bool logLoadedStats = false;

        private void OnEnable()
        {
            if (dashboardDataManager == null)
            {
                dashboardDataManager = DashboardDataManager.Instance;
            }

            if (dashboardDataManager == null)
            {
                Debug.LogWarning("PlayerStatsScreenUI: DashboardDataManager is missing.");
                return;
            }

            dashboardDataManager.OnDashboardDataLoaded += Render;

            if (dashboardDataManager.CurrentData != null)
            {
                Render(dashboardDataManager.CurrentData);
            }
        }

        private void OnDisable()
        {
            if (dashboardDataManager != null)
            {
                dashboardDataManager.OnDashboardDataLoaded -= Render;
            }
        }

        private void Render(DashboardData data)
        {
            if (data == null || data.profile == null)
            {
                return;
            }

            PlayerProfileData profile = data.profile;
            LevelInfoData levelInfo = data.levelInfo;

            if (logLoadedStats)
            {
                Debug.Log(
                    "[PlayerStatsScreenUI] " +
                    "profile.level=" + profile.level +
                    ", profile.xp=" + profile.xp +
                    ", totalDistanceRun=" + profile.totalDistanceRun +
                    ", totalTimeInGame=" + profile.totalTimeInGame +
                    ", totalJumps=" + profile.totalJumps +
                    ", levelInfo.level=" + (levelInfo != null ? levelInfo.level.ToString() : "null") +
                    ", currentLevelXp=" + (levelInfo != null ? levelInfo.currentLevelXp.ToString() : "null") +
                    ", nextLevelXp=" + (levelInfo != null ? levelInfo.nextLevelXp.ToString() : "null") +
                    ", xpIntoCurrentLevel=" + (levelInfo != null ? levelInfo.xpIntoCurrentLevel.ToString() : "null") +
                    ", xpNeededForNextLevel=" + (levelInfo != null ? levelInfo.xpNeededForNextLevel.ToString() : "null")
                );
            }

            int displayLevel = profile.level;

            if (levelInfo != null)
            {
                displayLevel = levelInfo.level;
            }

            SetText(levelText, "level : " + displayLevel);
            RenderXp(profile, levelInfo);

            SetText(totalActiveTimeText, FormatTime(profile.totalTimeInGame));
            SetText(totalDistanceText, FormatDistance(profile.totalDistanceRun));
            SetText(totalJumpsText, profile.totalJumps.ToString());
        }

        private void RenderXp(PlayerProfileData profile, LevelInfoData levelInfo)
        {
            if (levelInfo == null)
            {
                SetText(xpText, "xp : " + profile.xp);
                SetFill(xpFillImage, 0f);
                return;
            }

            int totalXp = profile.xp;
            int currentLevelXp = levelInfo.currentLevelXp;
            int nextLevelXp = levelInfo.nextLevelXp;

            if (nextLevelXp <= currentLevelXp)
            {
                SetText(xpText, "xp : MAX");
                SetFill(xpFillImage, 1f);
                return;
            }

            SetText(xpText, "xp : " + totalXp + " / " + nextLevelXp);

            float fill = 0f;

            if (nextLevelXp > 0)
            {
                fill = (float)totalXp / nextLevelXp;
            }

            SetFill(xpFillImage, fill);
        }

        private string FormatTime(float seconds)
        {
            int totalSeconds = Mathf.FloorToInt(seconds);
            int minutes = totalSeconds / 60;
            int remainingSeconds = totalSeconds % 60;

            return minutes + "m\n" + remainingSeconds + "s";
        }

        private string FormatDistance(float meters)
        {
            if (meters >= 1000f)
            {
                return (meters / 1000f).ToString("0.0") + "km";
            }

            return meters.ToString("0") + "m";
        }

        private void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private void SetFill(Image image, float value)
        {
            if (image != null)
            {
                image.fillAmount = Mathf.Clamp01(value);
            }
        }
    }
}
