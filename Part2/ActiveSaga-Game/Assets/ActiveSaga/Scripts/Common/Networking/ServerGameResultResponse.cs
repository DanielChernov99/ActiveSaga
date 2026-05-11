using System;

namespace ActiveSaga.Common.Networking
{
    [Serializable]
    public class ServerGameResultResponse
    {
        public bool success;
        public string message;

        public bool leveledUp;
        public int previousLevel;

        public ServerPlayerProgression player;
        public ServerRewardResult rewards;

        [NonSerialized] public string rawJson;
        [NonSerialized] public string errorMessage;
    }

    [Serializable]
    public class ServerPlayerProgression
    {
        public int level;
        public int currentXp;
        public int xpNeededForNextLevel;
        public int money;
        public int totalEarnedXp;
    }

    [Serializable]
    public class ServerRewardResult
    {
        public int gameplayXp;
        public int missionBonusXp;
        public int totalXp;

        public int gameplayMoney;
        public int missionBonusMoney;
        public int totalMoney;
    }
}