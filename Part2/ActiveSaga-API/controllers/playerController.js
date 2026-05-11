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

            return res.status(200).json({
                message: 'This game session was already processed',
                alreadyProcessed: true,
                session: existingSession,
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

        const totalXpEarned =
            gameRewards.xpEarned + questResult.questXpEarned;

        const totalCoinsEarned =
            gameRewards.coinsEarned + questResult.questCoinsEarned;

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
                totalXpEarned,
                totalCoinsEarned
            },

            completedQuests: questResult.completedQuests,
            levelBefore,
            levelAfter: profile.level
        });

        return res.status(200).json({
            message: 'Game session completed successfully',
            alreadyProcessed: false,

            rewards: {
                gameXpEarned: gameRewards.xpEarned,
                gameCoinsEarned: gameRewards.coinsEarned,
                questXpEarned: questResult.questXpEarned,
                questCoinsEarned: questResult.questCoinsEarned,
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