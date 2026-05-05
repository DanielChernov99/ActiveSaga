const mongoose = require('mongoose');

const playerProfileSchema = new mongoose.Schema({
    accountId: { 
        type: mongoose.Schema.Types.ObjectId, 
        ref: 'Account', 
        required: true 
    },
    
    firstName: { type: String, required: true },
    lastName: { type: String, required: true },
    
    level: { type: Number, default: 1 },
    xp: { type: Number, default: 0 },
    coins: { type: Number, default: 0 },

    totalDistanceRun: { type: Number, default: 0 }, 
    totalTimeInGame: { type: Number, default: 0 },
    
    inventory: { type: [String], default: [] },
    lastLogin: { type: Date, default: Date.now },

    dailyQuests: [{
        questId: { 
            type: mongoose.Schema.Types.ObjectId, 
            ref: 'Quest' 
        },
        isCompleted: { type: Boolean, default: false },
        currentProgress: { type: Number, default: 0 }, 
        lastUpdated: { type: Date, default: Date.now }
    }],

    lastQuestReset: { type: Date, default: Date.now }
});

module.exports = mongoose.model('PlayerProfile', playerProfileSchema);