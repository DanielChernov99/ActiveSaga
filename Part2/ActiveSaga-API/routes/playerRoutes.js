const express = require('express');
const router = express.Router();

const auth = require('../middleware/auth');
const playerController = require('../controllers/playerController');

router.get('/me', auth, playerController.getPlayerStats);

router.get('/daily-quests', auth, playerController.getDailyQuests);

router.get('/daily-streak', auth, playerController.getDailyStreak);

router.post('/complete-game-session', auth, playerController.completeGameSession);

router.post('/update-stats', auth, playerController.completeGameSession);

module.exports = router;