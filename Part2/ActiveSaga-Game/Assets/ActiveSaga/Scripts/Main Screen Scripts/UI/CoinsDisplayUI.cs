using ActiveSaga.MainScreen.Data;
using TMPro;
using UnityEngine;

namespace ActiveSaga.MainScreen.UI
{
    public class CoinsDisplayUI : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private DashboardDataManager dashboardDataManager;

        [Header("UI")]
        [SerializeField] private TMP_Text coinsText;

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
            if (data == null || data.profile == null || coinsText == null)
            {
                return;
            }

            coinsText.text = data.profile.coins.ToString();
        }
    }
}