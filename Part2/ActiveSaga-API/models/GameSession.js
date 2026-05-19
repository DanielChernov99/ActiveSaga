const mongoose = require('mongoose');

const gameSessionSchema = new mongoose.Schema({
    accountId: {
        type: mongoose.Schema.Types.ObjectId,
        ref: 'Account',
        required: true,
        index: true
    },

    clientSessionId: {
        type: String,
        required: true
    },

    gameType: {
        type: String,
        enum: ['RUN', 'FIGHT'],
        required: true
    },

    endReason: {
        type: String,
        enum: ['GAME_OVER', 'GAME_WON', 'QUIT', 'UNKNOWN'],
        default: 'UNKNOWN'
    },

    durationSeconds: {
        type: Number,
        default: 0
    },

    stats: {
    distanceRun: { type: Number, default: 0 },
    jumps: { type: Number, default: 0 },
    coinsCollected: { type: Number, default: 0 },
    enemiesKilled: { type: Number, default: 0 },
    dodges: { type: Number, default: 0 },
    wavesCompleted: { type: Number, default: 0 },
    obstaclesAvoided: { type: Number, default: 0 },
    bossDamageDealt: { type: Number, default: 0 }
    },

    rewards: {
        gameXpEarned: { type: Number, default: 0 },
        gameCoinsEarned: { type: Number, default: 0 },
        questXpEarned: { type: Number, default: 0 },
        questCoinsEarned: { type: Number, default: 0 },
        totalXpEarned: { type: Number, default: 0 },
        totalCoinsEarned: { type: Number, default: 0 }
    },

    completedQuests: [{
        questId: {
            type: mongoose.Schema.Types.ObjectId,
            ref: 'Quest'
        },
        title: String,
        xpReward: Number,
        coinsReward: Number
    }],

    levelBefore: {
        type: Number,
        default: 1
    },

    levelAfter: {
        type: Number,
        default: 1
    }
}, { timestamps: true });

gameSessionSchema.index(
    { accountId: 1, clientSessionId: 1 },
    { unique: true }
);

module.exports = mongoose.model('GameSession', gameSessionSchema);