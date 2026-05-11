using System.Threading.Tasks;
using UnityEngine;

namespace ActiveSaga.Common.Networking
{
    public class MockGameResultSubmitter : MonoBehaviour, IGameResultSubmitter
    {
        [Header("Mock Delay")]
        [SerializeField] private int delayMilliseconds = 500;

        [Header("Mock Rewards")]
        [SerializeField] private int mockGameplayXp = 120;
        [SerializeField] private int mockMissionBonusXp = 50;
        [SerializeField] private int mockGameplayMoney = 25;
        [SerializeField] private int mockMissionBonusMoney = 10;

        [Header("Mock Level Up")]
        [SerializeField] private bool mockLeveledUp = false;
        [SerializeField] private int mockPreviousLevel = 1;

        [Header("Mock Player State After Server")]
        [SerializeField] private int mockLevel = 2;
        [SerializeField] private int mockCurrentXp = 70;
        [SerializeField] private int mockXpNeededForNextLevel = 150;
        [SerializeField] private int mockMoney = 200;
        [SerializeField] private int mockTotalEarnedXp = 270;

        public async Task<ServerGameResultResponse> SubmitGameResultAsync(string jsonPayload)
        {
            await Task.Delay(delayMilliseconds);

            return new ServerGameResultResponse
            {
                success = true,
                message = "Mock server response",

                leveledUp = mockLeveledUp,
                previousLevel = mockPreviousLevel,

                player = new ServerPlayerProgression
                {
                    level = mockLevel,
                    currentXp = mockCurrentXp,
                    xpNeededForNextLevel = mockXpNeededForNextLevel,
                    money = mockMoney,
                    totalEarnedXp = mockTotalEarnedXp
                },

                rewards = new ServerRewardResult
                {
                    gameplayXp = mockGameplayXp,
                    missionBonusXp = mockMissionBonusXp,
                    totalXp = mockGameplayXp + mockMissionBonusXp,

                    gameplayMoney = mockGameplayMoney,
                    missionBonusMoney = mockMissionBonusMoney,
                    totalMoney = mockGameplayMoney + mockMissionBonusMoney
                },

                rawJson = jsonPayload,
                errorMessage = null
            };
        }
    }
}