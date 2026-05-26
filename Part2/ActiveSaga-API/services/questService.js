const Quest = require('../models/Quest');

const ONE_DAY_IN_MS = 24 * 60 * 60 * 1000;

function shouldRefreshDailyQuests(profile) {
    if (!profile.lastQuestReset) {
        return true;
    }

    if (!profile.dailyQuests || profile.dailyQuests.length === 0) {
        return true;
    }

    const now = new Date();
    return now - profile.lastQuestReset >= ONE_DAY_IN_MS;
}

async function getRandomQuestsByDifficulty(difficulty, count, playerLevel) {
    return Quest.aggregate([
        {
            $match: {
                difficulty,
                minLevel: { $lte: playerLevel }
            }
        },
        {
            $sample: {
                size: count
            }
        }
    ]);
}

async function generateDailyQuestsForProfile(profile) {
    let selectedQuests = [];

    if (profile.level <= 10) {
        const easy = await getRandomQuestsByDifficulty('EASY', 3, profile.level);
        selectedQuests = [...easy];
    } else if (profile.level <= 20) {
        const easy = await getRandomQuestsByDifficulty('EASY', 1, profile.level);
        const medium = await getRandomQuestsByDifficulty('MEDIUM', 2, profile.level);
        selectedQuests = [...easy, ...medium];
    } else {
        const easy = await getRandomQuestsByDifficulty('EASY', 1, profile.level);
        const medium = await getRandomQuestsByDifficulty('MEDIUM', 1, profile.level);
        const hard = await getRandomQuestsByDifficulty('HARD', 1, profile.level);
        selectedQuests = [...easy, ...medium, ...hard];
    }

    profile.dailyQuests = selectedQuests.map((quest) => ({
        questId: quest._id,
        isCompleted: false,
        currentProgress: 0,
        lastUpdated: new Date()
    }));

    profile.lastQuestReset = new Date();

    await profile.save();

    return profile.dailyQuests;
}

async function ensureDailyQuestsForToday(profile) {
    if (shouldRefreshDailyQuests(profile)) {
        await generateDailyQuestsForProfile(profile);
    }

    return profile.dailyQuests;
}

function getQuestProgressIncrement(quest, gameResult, gameRewards) {
    const { gameType, durationSeconds, stats } = gameResult;

    if (quest.gameType && quest.gameType !== 'ANY' && quest.gameType !== gameType) {
        return 0;
    }

    switch (quest.questType) {
        case 'DISTANCE':
            return stats.distanceRun;

        case 'TIME':
            return durationSeconds;

        case 'COINS':
            return stats.coinsCollected;

        case 'KILLS':
            return stats.enemiesKilled;

        case 'DODGES':
            return stats.dodges;

        case 'WAVES':
            return stats.wavesCompleted;

        case 'OBSTACLES':
            return stats.obstaclesAvoided;

        case 'RUN_GAMES':
            return gameType === 'RUN' ? 1 : 0;

        case 'FIGHT_GAMES':
            return gameType === 'FIGHT' ? 1 : 0;

        case 'GAMES_PLAYED':
            return 1;

        default:
            return 0;
    }
}

function updateDailyQuestProgress(profile, gameResult, gameRewards) {
    let questXpEarned = 0;
    let questCoinsEarned = 0;
    const completedQuests = [];

    for (const questEntry of profile.dailyQuests) {
        if (questEntry.isCompleted) {
            continue;
        }

        const quest = questEntry.questId;

        if (!quest) {
            continue;
        }

        const sessionValue = getQuestProgressIncrement(
            quest,
            gameResult,
            gameRewards
        );

        if (sessionValue <= 0) {
            continue;
        }

        questEntry.lastUpdated = new Date();

        const completedInThisSession = sessionValue >= quest.goalValue;

        if (!completedInThisSession) {
            questEntry.currentProgress = 0;
            continue;
        }

        questEntry.currentProgress = quest.goalValue;
        questEntry.isCompleted = true;

        profile.xp += quest.xpReward;
        profile.coins += quest.coinsReward;

        questXpEarned += quest.xpReward;
        questCoinsEarned += quest.coinsReward;

        completedQuests.push({
            questId: quest._id,
            title: quest.title,
            xpReward: quest.xpReward,
            coinsReward: quest.coinsReward
        });
    }

    return {
        questXpEarned,
        questCoinsEarned,
        completedQuests
    };
}

module.exports = {
    shouldRefreshDailyQuests,
    generateDailyQuestsForProfile,
    ensureDailyQuestsForToday,
    updateDailyQuestProgress
};