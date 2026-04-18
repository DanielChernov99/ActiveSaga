const PlayerProfile = require('../models/PlayerProfile');

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

// Controller function to update player stats
exports.updateStats = async (req, res) => {
    try {
        const accountId = req.user.accountId; 
        const { xpEarned, coinsEarned } = req.body;

        if (xpEarned === undefined || coinsEarned === undefined) {
            return res.status(400).json({ message: "Missing earned stats (xpEarned, coinsEarned)" });
        }

        const profile = await PlayerProfile.findOne({ accountId: accountId });

        if (!profile) {
            return res.status(404).json({ message: "Player profile not found" });
        }

        profile.xp += xpEarned;
        profile.coins += coinsEarned;

        
        // Level up logic based on XP thresholds 
        //level 1: 0 XP
        //level 2: 500 XP
        //level 3: 1500 XP   
        const xpThresholds = [0, 500, 1500, 3000, 5000, 8000, 12000, 18000, 25000, 35000];
        
        let calculatedLevel = 1;

        for (let i = 0; i < xpThresholds.length; i++) {
            if (profile.xp >= xpThresholds[i]) {
                calculatedLevel = i + 1; 
            }
        }
        
        if (calculatedLevel > profile.level) {
            profile.level = calculatedLevel;
            console.log(`Level Up! ${profile.firstName} is now level ${calculatedLevel}!`);
        }

        await profile.save();

        res.status(200).json({
            message: "Stats updated successfully",
            updatedStats: profile
        });

    } catch (error) {
        console.error("❌ Error updating player stats:", error);
        res.status(500).json({ message: "Server error during update" });
    }
};