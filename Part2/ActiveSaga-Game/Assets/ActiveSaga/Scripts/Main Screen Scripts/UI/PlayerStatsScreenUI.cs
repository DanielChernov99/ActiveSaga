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

        private void OnEnable()
        {
            if (dashboardDataManager == null)
            {
                dashboardDataManager = DashboardDataManager.Instance;
            }

            if (dashboardDataManager == null)
            {
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

            SetText(levelText, "level : " + profile.level);
            RenderXp(profile, levelInfo);

            SetText(totalActiveTimeText, FormatTime(profile.totalTimeInGame));
            SetText(totalDistanceText, FormatDistance(profile.totalDistanceRun));

            if (totalJumpsText != null)
            {
                totalJumpsText.text = "-";
            }
        }

        private void RenderXp(PlayerProfileData profile, LevelInfoData levelInfo)
        {
            if (levelInfo == null)
            {
                SetText(xpText, "xp : " + profile.xp);
                SetFill(xpFillImage, 0f);
                return;
            }

            int xpInsideCurrentLevel = levelInfo.xpIntoCurrentLevel;
            int currentLevelXp = levelInfo.currentLevelXp;
            int nextLevelXp = levelInfo.nextLevelXp;

            if (nextLevelXp <= currentLevelXp)
            {
                SetText(xpText, "xp : MAX");
                SetFill(xpFillImage, 1f);
                return;
            }

            int xpNeededForThisLevel = nextLevelXp - currentLevelXp;
            float fill = 0f;

            if (xpNeededForThisLevel > 0)
            {
                fill = (float)xpInsideCurrentLevel / xpNeededForThisLevel;
            }

            SetText(xpText, "xp : " + xpInsideCurrentLevel + " / " + xpNeededForThisLevel);
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