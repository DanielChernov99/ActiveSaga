using ActiveSaga.MainScreen.Data;
using TMPro;
using UnityEngine;

namespace ActiveSaga.MainScreen.UI
{
    public class WelcomeScreenUI : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private DashboardDataManager dashboardDataManager;

        [Header("UI")]
        [SerializeField] private TMP_Text welcomeText;

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
            if (data == null || data.profile == null || welcomeText == null)
            {
                return;
            }

            welcomeText.text = "Welcome, " + data.profile.firstName;
        }
    }
}