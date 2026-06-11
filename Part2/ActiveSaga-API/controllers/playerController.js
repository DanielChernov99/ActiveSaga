const PlayerProfile = require('../models/PlayerProfile');
const GameSession = require('../models/GameSession');

const {
    normalizeGameResult,
    calculateGameRewards,
    calculateLevelFromXp,
    applyGameResultToProfile
} = require('../services/progressionService');

const {
    ensureDailyQuestsForToday,
    generateDailyQuestsForProfile,
    updateDailyQuestProgress
} = require('../services/questService');

const {
    updateDailyStreakAfterGame,
    buildDailyStreakResponse
} = require('../services/dailyStreakService');

exports.getPlayerStats = async (req, res) => {
    try {
        const accountId = req.user.accountId;

        const profile = await PlayerProfile
            .findOne({ accountId })
            .populate('dailyQuests.questId');

        if (!profile) {
            return res.status(404).json({
                message: 'Player profile not found'
            });
        }

        const levelInfo = calculateLevelFromXp(profile.xp);

        return res.status(200).json({
            profile,
            levelInfo
        });
    } catch (error) {
        console.error('Error fetching player stats:', error);

        return res.status(500).json({
            message: 'Server error while fetching player stats',
            error: error.message
        });
    }
};

exports.getDailyQuests = async (req, res) => {
    try {
        const accountId = req.user.accountId;

        const profile = await PlayerProfile
            .findOne({ accountId })
            .populate('dailyQuests.questId');

        if (!profile) {
            return res.status(404).json({
                message: 'Player profile not found'
            });
        }

        await ensureDailyQuestsForToday(profile);

        await profile.populate('dailyQuests.questId');

        return res.status(200).json({
            message: 'Daily quests fetched successfully',
            quests: profile.dailyQuests,
            lastQuestReset: profile.lastQuestReset
        });
    } catch (error) {
        console.error('Error fetching daily quests:', error);

        return res.status(500).json({
            message: 'Server error while fetching daily quests',
            error: error.message
        });
    }
};

exports.getDailyStreak = async (req, res) => {
    try {
        const accountId = req.user.accountId;

        const profile = await PlayerProfile.findOne({ accountId });

        if (!profile) {
            return res.status(404).json({
                message: 'Player profile not found'
            });
        }

        const levelInfo = calculateLevelFromXp(profile.xp);
        const dailyStreak = buildDailyStreakResponse(profile, levelInfo);

        await profile.save();

        return res.status(200).json(dailyStreak);
    } catch (error) {
        console.error('Error fetching daily streak:', error);

        return res.status(500).json({
            message: 'Server error while fetching daily streak',
            error: error.message
        });
    }
};

exports.forceGenerateDailyQuests = async (req, res) => {
    try {
        const accountId = req.user.accountId;

        const profile = await PlayerProfile.findOne({ accountId });

        if (!profile) {
            return res.status(404).json({
                message: 'Player profile not found'
            });
        }

        await generateDailyQuestsForProfile(profile);

        await profile.populate('dailyQuests.questId');

        return res.status(200).json({
            message: 'New daily quests generated successfully',
            quests: profile.dailyQuests,
            lastQuestReset: profile.lastQuestReset
        });
    } catch (error) {
        console.error('Error generating daily quests:', error);

        return res.status(500).json({
            message: 'Server error while generating daily quests',
            error: error.message
        });
    }
};

exports.completeGameSession = async (req, res) => {
    try {
        const accountId = req.user.accountId;
        const gameResult = normalizeGameResult(req.body);

        if (!gameResult.clientSessionId) {
            return res.status(400).json({
                message: 'clientSessionId is required'
            });
        }

        if (!['RUN', 'FIGHT'].includes(gameResult.gameType)) {
            return res.status(400).json({
                message: 'gameType must be RUN or FIGHT'
            });
        }

        const existingSession = await GameSession.findOne({
            accountId,
            clientSessionId: gameResult.clientSessionId
        });

        if (existingSession) {
            const profile = await PlayerProfile
                .findOne({ accountId })
                .populate('dailyQuests.questId');

            const levelInfo = profile
                ? calculateLevelFromXp(profile.xp)
                : null;

            return res.status(200).json({
                success: true,
                message: 'This game session was already processed',
                alreadyProcessed: true,
                session: existingSession,
                rewards: existingSession.rewards,
                level: profile
                    ? {
                        before: existingSession.levelBefore,
                        after: existingSession.levelAfter,
                        leveledUp: existingSession.levelAfter > existingSession.levelBefore,
                        levelInfo
                    }
                    : null,
                dailyStreak: profile
                    ? buildDailyStreakResponse(profile, levelInfo)
                    : null,
                updatedStats: profile
            });
        }

        const profile = await PlayerProfile
            .findOne({ accountId })
            .populate('dailyQuests.questId');

        if (!profile) {
            return res.status(404).json({
                message: 'Player profile not found'
            });
        }

        await ensureDailyQuestsForToday(profile);
        await profile.populate('dailyQuests.questId');

        const levelBefore = profile.level;

        const gameRewards = calculateGameRewards(gameResult);

        applyGameResultToProfile(profile, gameResult, gameRewards);

        const questResult = updateDailyQuestProgress(
            profile,
            gameResult,
            gameRewards
        );

        const levelInfoBeforeStreak = calculateLevelFromXp(profile.xp);

        const streakResult = updateDailyStreakAfterGame(
            profile,
            gameResult,
            levelInfoBeforeStreak
        );

        profile.xp += streakResult.xpEarned;
        profile.coins += streakResult.coinsEarned;

        const totalXpEarned =
            gameRewards.xpEarned +
            questResult.questXpEarned +
            streakResult.xpEarned;

        const totalCoinsEarned =
            gameRewards.coinsEarned +
            questResult.questCoinsEarned +
            streakResult.coinsEarned;

        const levelInfo = calculateLevelFromXp(profile.xp);
        profile.level = levelInfo.level;

        await profile.save();

        const session = await GameSession.create({
            accountId,
            clientSessionId: gameResult.clientSessionId,
            gameType: gameResult.gameType,
            endReason: gameResult.endReason,
            durationSeconds: gameResult.durationSeconds,
            stats: gameResult.stats,

            rewards: {
                gameXpEarned: gameRewards.xpEarned,
                gameCoinsEarned: gameRewards.coinsEarned,

                questXpEarned: questResult.questXpEarned,
                questCoinsEarned: questResult.questCoinsEarned,

                streakXpEarned: streakResult.xpEarned,
                streakCoinsEarned: streakResult.coinsEarned,

                totalXpEarned,
                totalCoinsEarned
            },

            completedQuests: questResult.completedQuests,
            levelBefore,
            levelAfter: profile.level
        });

        return res.status(200).json({
            success: true,
            message: 'Game session completed successfully',
            alreadyProcessed: false,

            rewards: {
                gameXpEarned: gameRewards.xpEarned,
                gameCoinsEarned: gameRewards.coinsEarned,

                questXpEarned: questResult.questXpEarned,
                questCoinsEarned: questResult.questCoinsEarned,

                streakXpEarned: streakResult.xpEarned,
                streakCoinsEarned: streakResult.coinsEarned,

                totalXpEarned,
                totalCoinsEarned
            },

            level: {
                before: levelBefore,
                after: profile.level,
                leveledUp: profile.level > levelBefore,
                levelInfo
            },

            completedQuests: questResult.completedQuests,
            dailyStreak: buildDailyStreakResponse(profile, levelInfo),
            streakResult,
            updatedStats: profile,
            session
        });
    } catch (error) {
        if (error.code === 11000) {
            return res.status(409).json({
                message: 'Duplicate game session. This session was already saved.'
            });
        }

        console.error('Error completing game session:', error);

        return res.status(500).json({
            message: 'Server error during game session completion',
            error: error.message
        });
    }
};

// Backward compatibility for the old route name.
// Important: this no longer receives xpEarned or coinsEarned from Unity.
exports.updateStats = exports.completeGameSession;

// Backward compatibility for the old function name.
exports.generateDailyQuests = exports.getDailyQuests;