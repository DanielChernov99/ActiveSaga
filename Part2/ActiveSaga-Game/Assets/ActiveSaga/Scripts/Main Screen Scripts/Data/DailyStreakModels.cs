using System;

namespace ActiveSaga.MainScreen.Data
{
    [Serializable]
    public class DailyStreakResponse
    {
        public string weekStartDate;
        public int completedDaysCount;
        public int requiredDays;

        public TodayProgressDto todayProgress;

        public DailyRewardDto[] dailyRewards;
        public WeeklyRewardDto weeklyReward;
    }

    [Serializable]
    public class TodayProgressDto
    {
        public string date;
        public float playSeconds;
        public float requiredSeconds;
        public float remainingSeconds;
        public bool completedToday;
    }

    [Serializable]
    public class DailyRewardDto
    {
        public int slot;
        public bool completed;
        public int coinsReward;
    }

    [Serializable]
    public class WeeklyRewardDto
    {
        public bool completed;
        public bool claimed;
        public int xpReward;
        public int coinsReward;
    }
}