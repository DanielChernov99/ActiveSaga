const REQUIRED_PLAY_SECONDS = 5 * 60;
const REQUIRED_WEEKLY_DAYS = 5;

const DAILY_COIN_REWARDS = [
    10,
    20,
    30,
    40,
    50
];

const WEEKLY_COINS_REWARD = 200;
const WEEKLY_XP_PERCENT_OF_LEVEL = 0.1;

function getIsraelDateParts(date = new Date()) {
    const formatter = new Intl.DateTimeFormat('en-CA', {
        timeZone: 'Asia/Jerusalem',
        year: 'numeric',
        month: '2-digit',
        day: '2-digit'
    });

    const parts = formatter.formatToParts(date);

    const result = {
        year: 0,
        month: 0,
        day: 0
    };

    for (const part of parts) {
        if (part.type === 'year') {
            result.year = Number(part.value);
        }

        if (part.type === 'month') {
            result.month = Number(part.value);
        }

        if (part.type === 'day') {
            result.day = Number(part.value);
        }
    }

    return result;
}

function buildDateKey(year, month, day) {
    const paddedMonth = String(month).padStart(2, '0');
    const paddedDay = String(day).padStart(2, '0');

    return `${year}-${paddedMonth}-${paddedDay}`;
}

function getIsraelDateKey(date = new Date()) {
    const parts = getIsraelDateParts(date);
    return buildDateKey(parts.year, parts.month, parts.day);
}

function getIsraelWeekStartDateKey(date = new Date()) {
    const parts = getIsraelDateParts(date);

    const localCalendarDateAsUtc = new Date(
        Date.UTC(parts.year, parts.month - 1, parts.day)
    );

    const dayOfWeek = localCalendarDateAsUtc.getUTCDay();
    const sundayDate = new Date(localCalendarDateAsUtc);

    sundayDate.setUTCDate(sundayDate.getUTCDate() - dayOfWeek);

    return buildDateKey(
        sundayDate.getUTCFullYear(),
        sundayDate.getUTCMonth() + 1,
        sundayDate.getUTCDate()
    );
}

function ensureCurrentWeek(profile, now = new Date()) {
    const currentWeekStartDate = getIsraelWeekStartDateKey(now);

    if (!profile.weeklyStreak) {
        profile.weeklyStreak = {};
    }

    if (profile.weeklyStreak.weekStartDate !== currentWeekStartDate) {
        profile.weeklyStreak = {
            weekStartDate: currentWeekStartDate,
            dailyPlaySeconds: new Map(),
            completedDates: [],
            rewardedDates: [],
            weeklyRewardClaimed: false
        };

        return;
    }

    if (!profile.weeklyStreak.dailyPlaySeconds) {
        profile.weeklyStreak.dailyPlaySeconds = new Map();
    }

    if (!Array.isArray(profile.weeklyStreak.completedDates)) {
        profile.weeklyStreak.completedDates = [];
    }

    if (!Array.isArray(profile.weeklyStreak.rewardedDates)) {
        profile.weeklyStreak.rewardedDates = [];
    }

    if (typeof profile.weeklyStreak.weeklyRewardClaimed !== 'boolean') {
        profile.weeklyStreak.weeklyRewardClaimed = false;
    }
}

function getMapNumber(map, key) {
    if (!map) {
        return 0;
    }

    if (typeof map.get === 'function') {
        return Number(map.get(key) || 0);
    }

    return Number(map[key] || 0);
}

function setMapNumber(map, key, value) {
    if (typeof map.set === 'function') {
        map.set(key, value);
        return;
    }

    map[key] = value;
}

function getDailyRewardForSlot(slotNumber) {
    if (slotNumber < 1 || slotNumber > DAILY_COIN_REWARDS.length) {
        return 0;
    }

    return DAILY_COIN_REWARDS[slotNumber - 1];
}

function calculateWeeklyReward(levelInfo) {
    if (!levelInfo || levelInfo.nextLevelXp === null) {
        return {
            xpReward: 0,
            coinsReward: WEEKLY_COINS_REWARD
        };
    }

    const currentLevelXp = Number(levelInfo.currentLevelXp || 0);
    const nextLevelXp = Number(levelInfo.nextLevelXp || 0);
    const xpNeededForCurrentLevel = Math.max(0, nextLevelXp - currentLevelXp);

    const xpReward = Math.floor(xpNeededForCurrentLevel * WEEKLY_XP_PERCENT_OF_LEVEL);

    return {
        xpReward,
        coinsReward: WEEKLY_COINS_REWARD
    };
}

function updateDailyStreakAfterGame(profile, gameResult, levelInfo, now = new Date()) {
    ensureCurrentWeek(profile, now);

    const todayKey = getIsraelDateKey(now);
    const durationSeconds = Number(gameResult.durationSeconds || 0);

    const previousSeconds = getMapNumber(
        profile.weeklyStreak.dailyPlaySeconds,
        todayKey
    );

    const newSeconds = previousSeconds + Math.max(0, durationSeconds);

    setMapNumber(
        profile.weeklyStreak.dailyPlaySeconds,
        todayKey,
        newSeconds
    );

    let xpEarned = 0;
    let coinsEarned = 0;

    let dailyRewardClaimed = false;
    let dailyRewardSlot = 0;
    let dailyCoinsEarned = 0;

    let weeklyRewardClaimed = false;
    let weeklyXpEarned = 0;
    let weeklyCoinsEarned = 0;

    const alreadyCompletedToday =
        profile.weeklyStreak.completedDates.includes(todayKey);

    if (newSeconds >= REQUIRED_PLAY_SECONDS && !alreadyCompletedToday) {
        profile.weeklyStreak.completedDates.push(todayKey);

        const completedDaysCount = profile.weeklyStreak.completedDates.length;
        dailyRewardSlot = completedDaysCount;

        if (
            completedDaysCount <= REQUIRED_WEEKLY_DAYS &&
            !profile.weeklyStreak.rewardedDates.includes(todayKey)
        ) {
            dailyCoinsEarned = getDailyRewardForSlot(completedDaysCount);
            coinsEarned += dailyCoinsEarned;

            profile.weeklyStreak.rewardedDates.push(todayKey);
            dailyRewardClaimed = true;
        }
    }

    if (
        profile.weeklyStreak.completedDates.length >= REQUIRED_WEEKLY_DAYS &&
        !profile.weeklyStreak.weeklyRewardClaimed
    ) {
        const weeklyReward = calculateWeeklyReward(levelInfo);

        weeklyXpEarned = weeklyReward.xpReward;
        weeklyCoinsEarned = weeklyReward.coinsReward;

        xpEarned += weeklyXpEarned;
        coinsEarned += weeklyCoinsEarned;

        profile.weeklyStreak.weeklyRewardClaimed = true;
        weeklyRewardClaimed = true;
    }

    if (typeof profile.markModified === 'function') {
        profile.markModified('weeklyStreak');
    }

    return {
        xpEarned,
        coinsEarned,

        dailyRewardClaimed,
        dailyRewardSlot,
        dailyCoinsEarned,

        weeklyRewardClaimed,
        weeklyXpEarned,
        weeklyCoinsEarned,

        completedDaysCount: Math.min(
            REQUIRED_WEEKLY_DAYS,
            profile.weeklyStreak.completedDates.length
        ),
        requiredDays: REQUIRED_WEEKLY_DAYS,
        todayPlaySeconds: newSeconds
    };
}

function buildDailyStreakResponse(profile, levelInfo, now = new Date()) {
    ensureCurrentWeek(profile, now);

    const todayKey = getIsraelDateKey(now);

    const todayPlaySeconds = getMapNumber(
        profile.weeklyStreak.dailyPlaySeconds,
        todayKey
    );

    const todayRemainingSeconds = Math.max(
        0,
        REQUIRED_PLAY_SECONDS - todayPlaySeconds
    );

    const completedToday =
        profile.weeklyStreak.completedDates.includes(todayKey);

    const completedDaysCount = Math.min(
        REQUIRED_WEEKLY_DAYS,
        profile.weeklyStreak.completedDates.length
    );

    const dailyRewards = [];

    for (let i = 1; i <= REQUIRED_WEEKLY_DAYS; i++) {
        dailyRewards.push({
            slot: i,
            completed: i <= completedDaysCount,
            coinsReward: getDailyRewardForSlot(i)
        });
    }

    const weeklyReward = calculateWeeklyReward(levelInfo);

    return {
        weekStartDate: profile.weeklyStreak.weekStartDate,
        completedDaysCount,
        requiredDays: REQUIRED_WEEKLY_DAYS,

        todayProgress: {
            date: todayKey,
            playSeconds: todayPlaySeconds,
            requiredSeconds: REQUIRED_PLAY_SECONDS,
            remainingSeconds: todayRemainingSeconds,
            completedToday
        },

        dailyRewards,

        weeklyReward: {
            completed: completedDaysCount >= REQUIRED_WEEKLY_DAYS,
            claimed: profile.weeklyStreak.weeklyRewardClaimed,
            xpReward: weeklyReward.xpReward,
            coinsReward: weeklyReward.coinsReward
        }
    };
}

module.exports = {
    REQUIRED_PLAY_SECONDS,
    REQUIRED_WEEKLY_DAYS,
    updateDailyStreakAfterGame,
    buildDailyStreakResponse
};