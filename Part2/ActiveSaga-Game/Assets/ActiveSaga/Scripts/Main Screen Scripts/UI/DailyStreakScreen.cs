using ActiveSaga.Common.Networking;
using ActiveSaga.MainScreen.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ActiveSaga.MainScreen.UI
{
    public class DailyStreakScreen : MonoBehaviour
    {
        [Header("API")]
        [SerializeField] private PlayerApiService playerApiService;

        [Header("Day Images")]
        [SerializeField] private Image[] dayImages;

        [Header("Locked Sprites")]
        [SerializeField] private Sprite[] lockedSprites;

        [Header("Reward Sprites")]
        [SerializeField] private Sprite[] rewardSprites;

        [Header("Optional")]
        [SerializeField] private TMP_Text statusText;

        private void OnEnable()
        {
            LoadDailyStreak();
        }

        public void LoadDailyStreak()
        {
            if (playerApiService == null)
            {
                Debug.LogError("[DailyStreakScreen] Missing PlayerApiService reference.");
                return;
            }

            StartCoroutine(playerApiService.GetDailyStreak(
                ApplyDailyStreak,
                HandleDailyStreakError
            ));
        }

        private void ApplyDailyStreak(DailyStreakResponse response)
        {
            if (response == null)
            {
                Debug.LogError("[DailyStreakScreen] Response is null.");
                return;
            }

            if (response.dailyRewards == null)
            {
                Debug.LogError("[DailyStreakScreen] dailyRewards is null.");
                return;
            }

            for (int i = 0; i < dayImages.Length; i++)
            {
                if (dayImages[i] == null)
                {
                    continue;
                }

                bool completed = false;

                if (i < response.dailyRewards.Length)
                {
                    completed = response.dailyRewards[i].completed;
                }

                if (completed)
                {
                    if (i < rewardSprites.Length && rewardSprites[i] != null)
                    {
                        dayImages[i].sprite = rewardSprites[i];
                    }
                }
                else
                {
                    if (i < lockedSprites.Length && lockedSprites[i] != null)
                    {
                        dayImages[i].sprite = lockedSprites[i];
                    }
                }
            }

            if (statusText != null)
            {
                statusText.text = response.completedDaysCount + " / " + response.requiredDays;
            }

            Debug.Log("[DailyStreakScreen] Daily streak loaded successfully.");
        }

        private void HandleDailyStreakError(string error)
        {
            Debug.LogError("[DailyStreakScreen] Failed to load daily streak:\n" + error);

            if (statusText != null)
            {
                statusText.text = "Failed to load";
            }
        }
    }
}