using System.Threading.Tasks;
using UnityEngine;

namespace ActiveSaga.Common.Networking
{
    public class MockGameResultSubmitter : MonoBehaviour, IGameResultSubmitter
    {
        [Header("Mock Settings")]
        [SerializeField] private float fakeDelaySeconds = 1f;

        [Header("Mock Rewards")]
        [SerializeField] private int gameXpEarned = 50;
        [SerializeField] private int gameCoinsEarned = 20;
        [SerializeField] private int questXpEarned = 25;
        [SerializeField] private int questCoinsEarned = 10;

        [Header("Mock Progression")]
        [SerializeField] private int levelBefore = 1;
        [SerializeField] private int levelAfter = 1;
        [SerializeField] private int totalPlayerXp = 150;
        [SerializeField] private int totalPlayerCoins = 80;
        [SerializeField] private int xpIntoCurrentLevel = 150;
        [SerializeField] private int xpNeededForNextLevel = 350;

        public async Task<ServerGameResultResponse> SubmitGameResultAsync(string jsonPayload)
        {
            if (fakeDelaySeconds > 0f)
            {
                int delayMs = Mathf.RoundToInt(fakeDelaySeconds * 1000f);
                await Task.Delay(delayMs);
            }

            int totalXpEarned = gameXpEarned + questXpEarned;
            int totalCoinsEarned = gameCoinsEarned + questCoinsEarned;

            return new ServerGameResultResponse
            {
                success = true,
                message = "Mock game session completed successfully",
                alreadyProcessed = false,

                rewards = new ServerRewardResult
                {
                    gameXpEarned = gameXpEarned,
                    gameCoinsEarned = gameCoinsEarned,

                    questXpEarned = questXpEarned,
                    questCoinsEarned = questCoinsEarned,

                    totalXpEarned = totalXpEarned,
                    totalCoinsEarned = totalCoinsEarned
                },

                level = new ServerLevelResult
                {
                    before = levelBefore,
                    after = levelAfter,
                    leveledUp = levelAfter > levelBefore,

                    levelInfo = new ServerLevelInfo
                    {
                        level = levelAfter,
                        currentLevelXp = 0,
                        nextLevelXp = xpIntoCurrentLevel + xpNeededForNextLevel,
                        xpIntoCurrentLevel = xpIntoCurrentLevel,
                        xpNeededForNextLevel = xpNeededForNextLevel
                    }
                },

                updatedStats = new ServerUpdatedStats
                {
                    level = levelAfter,
                    xp = totalPlayerXp,
                    coins = totalPlayerCoins,
                    totalDistanceRun = 0f,
                    totalTimeInGame = 0f,
                    totalJumps = 0
                },

                rawJson = "",
                errorMessage = ""
            };
        }
    }
}