using System;

namespace ActiveSaga.Common.Networking
{
    [Serializable]
    public class ServerGameResultResponse
    {
        public bool success;
        public string message;
        public bool alreadyProcessed;

        public ServerRewardResult rewards;
        public ServerLevelResult level;
        public ServerUpdatedStats updatedStats;

        [NonSerialized] public string rawJson;
        [NonSerialized] public string errorMessage;
    }

    [Serializable]
    public class ServerRewardResult
    {
        public int gameXpEarned;
        public int gameCoinsEarned;

        public int questXpEarned;
        public int questCoinsEarned;

        public int totalXpEarned;
        public int totalCoinsEarned;
    }

    [Serializable]
    public class ServerLevelResult
    {
        public int before;
        public int after;
        public bool leveledUp;
        public ServerLevelInfo levelInfo;
    }

    [Serializable]
    public class ServerLevelInfo
    {
        public int level;
        public int currentLevelXp;
        public int nextLevelXp;
        public int xpIntoCurrentLevel;
        public int xpNeededForNextLevel;
    }

    [Serializable]
    public class ServerUpdatedStats
    {
        public int level;
        public int xp;
        public int coins;
        public float totalDistanceRun;
        public float totalTimeInGame;
        public int totalJumps;
    }
}