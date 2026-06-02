using UnityEngine;
using ActiveSaga.BossFight.Data;
using ActiveSaga.BossFight.Entities;
using ActiveSaga.FightGame;

namespace ActiveSaga.BossFight.Waves
{
    public class WaveEvaluator
    {
        public bool EvaluateWave(
            WaveData data,
            int totalSpawnedThisWave,
            int successfullyHandledThisWave,
            int playerHitCountThisWave,
            float bossDamagePerSuccessfulWave)
        {
            if (data == null)
            {
                Debug.LogError("[WaveEvaluator] Cannot evaluate wave because WaveData is null.");
                return false;
            }

            Debug.Log($"EvaluateWave called for {data.waveName}. Total Spawned: {totalSpawnedThisWave}, Successfully Handled: {successfullyHandledThisWave}");

            if (totalSpawnedThisWave == 0)
            {
                return false;
            }

            float successRate = (float)successfullyHandledThisWave / totalSpawnedThisWave;
            bool success = successRate >= 0.8f;

            Debug.Log($"Wave {data.waveType} Result: Handled {successfullyHandledThisWave}/{totalSpawnedThisWave} ({successRate:P0}). Hit: {playerHitCountThisWave}");

            if (success)
            {
                Debug.Log("<color=green>Wave Success! Damaging Boss.</color>");

                if (BossController.Instance != null)
                {
                    BossController.Instance.TakeDamage(bossDamagePerSuccessfulWave);

                    FightGameStatsTracker statsTracker = Object.FindFirstObjectByType<FightGameStatsTracker>();

                    if (statsTracker != null)
                    {
                        statsTracker.AddBossDamage(bossDamagePerSuccessfulWave);
                    }
                    else
                    {
                        Debug.LogWarning("[WaveEvaluator] FightGameStatsTracker was not found, boss damage was not recorded in stats.");
                    }
                }
                else
                {
                    Debug.LogError("[WaveEvaluator] CANNOT DAMAGE BOSS: BossController.Instance is NULL!");
                }
            }
            else
            {
                Debug.Log("<color=red>Wave Failed. No Boss damage.</color>");
            }

            return success;
        }
    }
}