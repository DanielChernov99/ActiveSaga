const mongoose = require('mongoose');

const dailyQuestProgressSchema = new mongoose.Schema({
    questId: {
        type: mongoose.Schema.Types.ObjectId,
        ref: 'Quest',
        required: true
    },

    isCompleted: {
        type: Boolean,
        default: false
    },

    currentProgress: {
        type: Number,
        default: 0
    },

    lastUpdated: {
        type: Date,
        default: Date.now
    }
}, { _id: false });

const weeklyStreakSchema = new mongoose.Schema({
    weekStartDate: {
        type: String,
        default: ''
    },

    dailyPlaySeconds: {
        type: Map,
        of: Number,
        default: {}
    },

    completedDates: {
        type: [String],
        default: []
    },

    rewardedDates: {
        type: [String],
        default: []
    },

    weeklyRewardClaimed: {
        type: Boolean,
        default: false
    }
}, { _id: false });

const playerProfileSchema = new mongoose.Schema({
    accountId: {
        type: mongoose.Schema.Types.ObjectId,
        ref: 'Account',
        required: true,
        unique: true,
        index: true
    },

    firstName: {
        type: String,
        required: true
    },

    lastName: {
        type: String,
        required: true
    },

    level: {
        type: Number,
        default: 1
    },

    xp: {
        type: Number,
        default: 0
    },

    coins: {
        type: Number,
        default: 0
    },

    totalDistanceRun: {
        type: Number,
        default: 0
    },

    totalTimeInGame: {
        type: Number,
        default: 0
    },

    totalJumps: {
    type: Number,
    default: 0
    },


    inventory: {
        type: [String],
        default: []
    },

    lastLogin: {
        type: Date,
        default: Date.now
    },

    dailyQuests: {
        type: [dailyQuestProgressSchema],
        default: []
    },

    lastQuestReset: {
        type: Date,
        default: Date.now
    },

    weeklyStreak: {
        type: weeklyStreakSchema,
        default: () => ({})
    }

}, { timestamps: true });

module.exports = mongoose.model('PlayerProfile', playerProfileSchema);