const express = require('express');
const router = express.Router();

const auth = require('../middleware/auth');
const playerController = require('../controllers/playerController');

// GET request to fetch current player stats
router.get('/me', auth, playerController.getPlayerStats);
// POST request to update stats after a match ends
router.post('/update-stats', auth, playerController.updateStats);

module.exports = router;