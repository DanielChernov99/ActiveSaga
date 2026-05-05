const PlayerProfile = require('../models/PlayerProfile');
const Quest = require('../models/Quest');

// Controller function to get player stats
exports.getPlayerStats = async (req, res) => {
    try {
        const accountId = req.user.accountId;

        const profile = await PlayerProfile.findOne({ accountId: accountId });

        if (!profile) {
            return res.status(404).json({ message: "Player profile not found" });
        }

        res.status(200).json(profile);

    } catch (error) {
        console.error("❌ Error fetching player stats:", error);
        res.status(500).json({ message: "Server error" });
    }
};

/**
 * Updates player statistics and daily quest progress after a game session.
 * 
 * Logic:
 * 1. Validates input from the request body.
 * 2. Updates general profile stats (XP, Coins, Distance, Time).
 * 3. Iterates through active daily quests to update progress based on questType.
 * 4. Automatically marks quests as completed and grants rewards if goals are met.
 * 5. Recalculates player level based on total XP.
 */
exports.updateStats = async (req, res) => {
    try {
        const accountId = req.user.accountId; 
        const { xpEarned, coinsEarned, distanceRun, timePlayed } = req.body;

        // Validation: Ensure core gameplay stats are provided
        if (xpEarned === undefined || coinsEarned === undefined) {
            return res.status(400).json({ message: "Missing earned stats (xpEarned, coinsEarned)" });
        }

        // Fetch profile and populate quest details to access 'goalValue' and 'questType'
        const profile = await PlayerProfile.findOne({ accountId: accountId }).populate('dailyQuests.questId');

        if (!profile) {
            return res.status(404).json({ message: "Player profile not found" });
        }

        // --- 1. Update Core Player Stats ---
        profile.xp += (xpEarned || 0);
        profile.coins += (coinsEarned || 0);
        profile.totalDistanceRun += (distanceRun || 0);
        profile.totalTimeInGame += (timePlayed || 0);

        // --- 2. Update Daily Quest Progress ---
        // We loop through the user's active quests to see if this run contributed to them[cite: 2, 5]
        profile.dailyQuests.forEach(questEntry => {
            // Only update quests that aren't already finished
            if (questEntry.isCompleted) return;

            const quest = questEntry.questId; // This is the populated Quest document
            if (!quest) return;

            // Update progress based on the specific type of the quest
            switch (quest.questType) {
                case 'DISTANCE':
                    questEntry.currentProgress += (distanceRun || 0);
                    break;
                case 'COINS':
                    questEntry.currentProgress += (coinsEarned || 0);
                    break;
                case 'TIME':
                    questEntry.currentProgress += (timePlayed || 0);
                    break;
            }

            // Check if the player reached the goal during this session
            if (questEntry.currentProgress >= quest.goalValue) {
                questEntry.isCompleted = true;
                
                // Grant bonus rewards from the quest itself
                profile.xp += quest.xpReward;
                profile.coins += quest.coinsReward;
                
                console.log(`✨ Quest Complete: ${quest.title} (+${quest.xpReward} XP, +${quest.coinsReward} Coins)`);
            }
        });

        // --- 3. Level Up Logic ---
        // XP Thresholds for progression
        const xpThresholds = [0, 500, 1500, 3000, 5000, 8000, 12000, 18000, 25000, 35000];
        let calculatedLevel = 1;

        for (let i = 0; i < xpThresholds.length; i++) {
            if (profile.xp >= xpThresholds[i]) {
                calculatedLevel = i + 1; 
            }
        }
        
        if (calculatedLevel > profile.level) {
            profile.level = calculatedLevel;
            console.log(`🚀 Level Up! ${profile.firstName} reached level ${calculatedLevel}!`);
        }

        // Save all changes to the database
        await profile.save();

        res.status(200).json({
            message: "Stats and quest progress updated successfully",
            updatedStats: profile
        });

    } catch (error) {
        console.error("❌ Error updating player stats:", error);
        res.status(500).json({ message: "Server error during stats update", error: error.message });
    }
};


/**
 * Handles the generation and retrieval of daily quests.
 * Logic: 
 * 1. Checks if 24 hours have passed since the last reset.
 * 2. If not, returns existing quests.
 * 3. If yes, generates 3 new quests based on player level:
 *    - Level < 10: 3 EASY
 *    - Level 10-20: 2 MEDIUM, 1 EASY
 *    - Level 20+: 1 EASY, 1 MEDIUM, 1 HARD
 */
exports.generateDailyQuests = async (req, res) => {
    try {
        const accountId = req.user.accountId; 
        
        // Find profile and populate the questId to get full quest details (title, goal, etc.)
        const profile = await PlayerProfile.findOne({ accountId }).populate('dailyQuests.questId');

        if (!profile) {
            return res.status(404).json({ message: "Player profile not found" });
        }

        const now = new Date();
        const oneDayInMs = 24 * 60 * 60 * 1000;
        
        // Check if quests need to be refreshed (first time or 24h passed)
        const needsRefresh = !profile.lastQuestReset || (now - profile.lastQuestReset) > oneDayInMs;

        if (!needsRefresh && profile.dailyQuests.length > 0) {
            return res.status(200).json({
                message: "Fetched existing daily quests",
                quests: profile.dailyQuests
            });
        }

        // --- Start Generation Logic ---
        let selectedQuests = [];

        /**
         * Helper function to fetch random documents from the Quest collection
         * @param {String} difficulty - 'EASY', 'MEDIUM', or 'HARD'
         * @param {Number} count - Number of quests to pull
         */
        const getRandomQuests = async (difficulty, count) => {
            return await Quest.aggregate([
                { $match: { difficulty: difficulty } },
                { $sample: { size: count } }
            ]);
        };

        // Algorithm based on user requirements for level progression[cite: 2]
        if (profile.level < 10) {
            // Level 0-9: 3 Easy Quests
            selectedQuests = await getRandomQuests('EASY', 3);
        } 
        else if (profile.level >= 10 && profile.level < 20) {
            // Level 10-19: 2 Medium + 1 Easy
            const easy = await getRandomQuests('EASY', 1);
            const medium = await getRandomQuests('MEDIUM', 2);
            selectedQuests = [...easy, ...medium];
        } 
        else {
            // Level 20+: 1 Easy + 1 Medium + 1 Hard
            const easy = await getRandomQuests('EASY', 1);
            const medium = await getRandomQuests('MEDIUM', 1);
            const hard = await getRandomQuests('HARD', 1);
            selectedQuests = [...easy, ...medium, ...hard];
        }

        // Map the selected quests into the player's profile structure[cite: 2]
        profile.dailyQuests = selectedQuests.map(q => ({
            questId: q._id,
            isCompleted: false,
            currentProgress: 0
        }));

        profile.lastQuestReset = now;
        await profile.save();

        // Re-populate after saving to send full data back to Unity
        const updatedProfile = await PlayerProfile.findOne({ accountId }).populate('dailyQuests.questId');

        res.status(200).json({
            message: "New daily quests generated successfully",
            quests: updatedProfile.dailyQuests
        });

    } catch (error) {
        console.error("Quest Generation Error:", error);
        res.status(500).json({ message: "Server error during quest generation", error: error.message });
    }
};