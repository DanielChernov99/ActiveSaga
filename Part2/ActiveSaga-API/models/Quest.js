const mongoose = require('mongoose');

const QuestSchema = new mongoose.Schema({
    title: { type: String, required: true }, 
    description: { type: String, required: true },
    difficulty: { 
        type: String, 
        enum: ['EASY', 'MEDIUM', 'HARD'], 
        required: true 
    },
    minLevel: { type: Number, required: true }, 
    xpReward: { type: Number, required: true },
    coinsReward: { type: Number, required: true },
    goalValue: { type: Number, required: true },
    questType: { 
        type: String, 
        enum: ['DISTANCE', 'COINS', 'TIME'], 
        required: true 
    }
});

module.exports = mongoose.model('Quest', QuestSchema);