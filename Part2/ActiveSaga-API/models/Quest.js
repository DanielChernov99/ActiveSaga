const mongoose = require('mongoose');

const questSchema = new mongoose.Schema({
    title: {
        type: String,
        required: true
    },

    description: {
        type: String,
        required: true
    },

    difficulty: {
        type: String,
        enum: ['EASY', 'MEDIUM', 'HARD'],
        required: true
    },

    minLevel: {
        type: Number,
        required: true,
        default: 1
    },

    xpReward: {
        type: Number,
        required: true,
        default: 0
    },

    coinsReward: {
        type: Number,
        required: true,
        default: 0
    },

    goalValue: {
        type: Number,
        required: true
    },

    questType: {
        type: String,
        enum: [
            'DISTANCE',
            'TIME',
            'COINS',
            'KILLS',
            'DODGES',
            'WAVES',
            'OBSTACLES',
            'RUN_GAMES',
            'FIGHT_GAMES',
            'GAMES_PLAYED'
        ],
        required: true
    },

    gameType: {
        type: String,
        enum: ['ANY', 'RUN', 'FIGHT'],
        default: 'ANY'
    }
}, { timestamps: true });

module.exports = mongoose.model('Quest', questSchema);