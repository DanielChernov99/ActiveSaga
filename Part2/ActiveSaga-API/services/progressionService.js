const LEVEL_THRESHOLDS = [
    0,
    500,
    1500,
    3000,
    5000,
    8000,
    12000,
    18000,
    25000,
    35000,
    50000,
    70000,
    95000,
    125000,
    160000,
    200000,
    250000,
    310000,
    380000,
    460000,
    550000,
    650000,
    770000,
    900000,
    1050000,
    1220000,
    1410000,
    1620000,
    1850000,
    2100000
];



function toSafeNumber(value) {
    const numberValue = Number(value);

    if (!Number.isFinite(numberValue) || numberValue < 0) {
        return 0;
    }

    return numberValue;
}

function normalizeGameResult(body) {
    const stats = body.stats || {};

    return {
        clientSessionId: String(body.clientSessionId || '').trim(),
        gameType: String(body.gameType || '').toUpperCase(),
        endReason: String(body.endReason || 'UNKNOWN').toUpperCase(),
        durationSeconds: toSafeNumber(body.durationSeconds),

        stats: {
        distanceRun: toSafeNumber(stats.distanceRun),
        jumps: toSafeNumber(stats.jumps),
        coinsCollected: toSafeNumber(stats.coinsCollected),
        enemiesKilled: toSafeNumber(stats.enemiesKilled),
        dodges: toSafeNumber(stats.dodges),
        wavesCompleted: toSafeNumber(stats.wavesCompleted),
        obstaclesAvoided: toSafeNumber(stats.obstaclesAvoided),
        bossDamageDealt: toSafeNumber(stats.bossDamageDealt)
        }
    };
}

const COIN_VALUE = 10;

function calculateGameRewards(gameResult) {
    const { gameType, durationSeconds, stats } = gameResult;

    let xp = 0;
    let coins = 0;

    const timeXp = Math.floor(durationSeconds / 10) * 2;
    const timeCoins = Math.floor(durationSeconds / 30);

    if (gameType === 'RUN') {
        xp += Math.floor(stats.distanceRun * 0.1);
        xp += stats.enemiesKilled * 8;
        xp += stats.obstaclesAvoided * 3;
        xp += stats.jumps * 1;
        xp += timeXp;

        coins += stats.coinsCollected * COIN_VALUE;
    }

    if (gameType === 'FIGHT') {
        xp += stats.enemiesKilled * 10;
        xp += stats.dodges * 4;
        xp += stats.wavesCompleted * 25;
        xp += Math.floor(stats.bossDamageDealt * 0.05);
        xp += timeXp;

        coins += stats.enemiesKilled * 2;
        coins += stats.dodges;
        coins += stats.wavesCompleted * 5;
        coins += timeCoins;
    }

    return {
        xpEarned: Math.max(0, Math.floor(xp)),
        coinsEarned: Math.max(0, Math.floor(coins))
    };
}

function calculateLevelFromXp(totalXp) {
    let level = 1;

    for (let i = 0; i < LEVEL_THRESHOLDS.length; i++) {
        if (totalXp >= LEVEL_THRESHOLDS[i]) {
            level = i + 1;
        }
    }

    if (level > 30) {
        level = 30;
    }

    const currentLevelXp = LEVEL_THRESHOLDS[level - 1] || 0;
    const nextLevelXp = LEVEL_THRESHOLDS[level] || null;

    return {
        level,
        currentLevelXp,
        nextLevelXp,
        xpIntoCurrentLevel: totalXp - currentLevelXp,
        xpNeededForNextLevel: nextLevelXp === null ? null : nextLevelXp - totalXp
    };
}

function applyGameResultToProfile(profile, gameResult, gameRewards) {
    profile.xp += gameRewards.xpEarned;
    profile.coins += gameRewards.coinsEarned;

    profile.totalTimeInGame += gameResult.durationSeconds;

    if (gameResult.gameType === 'RUN') {
        profile.totalDistanceRun += gameResult.stats.distanceRun;
        profile.totalJumps += gameResult.stats.jumps;
    }
}

module.exports = {
    LEVEL_THRESHOLDS,
    normalizeGameResult,
    calculateGameRewards,
    calculateLevelFromXp,
    applyGameResultToProfile
};