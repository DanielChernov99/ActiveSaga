using System;

namespace ActiveSaga.MainScreen.Data
{
    [Serializable]
    public class DashboardData
    {
        public PlayerProfileData profile;
        public LevelInfoData levelInfo;
        public DailyQuestEntryData[] dailyQuests;
        public string lastQuestReset;
    }

    [Serializable]
    public class PlayerStatsResponse
    {
        public PlayerProfileData profile;
        public LevelInfoData levelInfo;
    }

    [Serializable]
    public class DailyQuestsResponse
    {
        public string message;
        public DailyQuestEntryData[] quests;
        public string lastQuestReset;
    }

    [Serializable]
    public class PlayerProfileData
    {
        public string _id;
        public string accountId;

        public string firstName;
        public string lastName;

        public int level;
        public int xp;
        public int coins;

        public float totalDistanceRun;
        public float totalTimeInGame;

        public DailyQuestEntryData[] dailyQuests;
        public string lastQuestReset;
    }

    [Serializable]
    public class LevelInfoData
    {
        public int level;
        public int currentLevelXp;
        public int nextLevelXp;
        public int xpIntoCurrentLevel;
        public int xpNeededForNextLevel;
    }

    [Serializable]
    public class DailyQuestEntryData
    {
        public QuestData questId;
        public bool isCompleted;
        public float currentProgress;
        public string lastUpdated;
    }

    [Serializable]
    public class QuestData
    {
        public string _id;

        public string title;
        public string description;

        public string difficulty;
        public int minLevel;

        public int xpReward;
        public int coinsReward;

        public float goalValue;

        public string questType;
        public string gameType;
    }
}