using System;
using System.Collections;
using ActiveSaga.Common.Networking;
using UnityEngine;

namespace ActiveSaga.MainScreen.Data
{
    public class DashboardDataManager : MonoBehaviour
    {
        public static DashboardDataManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private PlayerApiService playerApiService;

        [Header("Settings")]
        [SerializeField] private bool loadOnStart = true;

        public DashboardData CurrentData { get; private set; }

        public event Action<DashboardData> OnDashboardDataLoaded;
        public event Action<string> OnDashboardDataFailed;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (playerApiService == null)
            {
                playerApiService = GetComponent<PlayerApiService>();
            }
        }

        private void Start()
        {
            if (loadOnStart)
            {
                LoadDashboardData();
            }
        }

        public void LoadDashboardData()
        {
            StartCoroutine(LoadDashboardDataRoutine());
        }

        private IEnumerator LoadDashboardDataRoutine()
        {
            if (playerApiService == null)
            {
                Fail("PlayerApiService is missing on UI_Manager.");
                yield break;
            }

            PlayerStatsResponse statsResponse = null;
            DailyQuestsResponse questsResponse = null;
            string error = "";

            yield return playerApiService.GetPlayerStats(
                response => statsResponse = response,
                errorMessage => error = errorMessage
            );

            if (!string.IsNullOrEmpty(error))
            {
                Fail(error);
                yield break;
            }

            yield return playerApiService.GetDailyQuests(
                response => questsResponse = response,
                errorMessage => error = errorMessage
            );

            if (!string.IsNullOrEmpty(error))
            {
                Fail(error);
                yield break;
            }

            CurrentData = new DashboardData
            {
                profile = statsResponse.profile,
                levelInfo = statsResponse.levelInfo,
                dailyQuests = questsResponse.quests,
                lastQuestReset = questsResponse.lastQuestReset
            };

            Debug.Log("Dashboard data loaded successfully.");

            OnDashboardDataLoaded?.Invoke(CurrentData);
        }

        private void Fail(string error)
        {
            Debug.LogError("Dashboard data loading failed:\n" + error);
            OnDashboardDataFailed?.Invoke(error);
        }
    }
}