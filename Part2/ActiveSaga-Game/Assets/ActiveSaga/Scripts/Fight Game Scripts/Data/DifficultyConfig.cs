using UnityEngine;
using ActiveSaga.Common.GameSession;

namespace ActiveSaga.BossFight.Data
{
    [CreateAssetMenu(fileName = "DifficultyConfig", menuName = "BossFight/DifficultyConfig")]
    public class DifficultyConfig : ScriptableObject
    {
        [Header("Dodge Wave Count Settings")]
        [SerializeField] private int easyEntityCount = 5;
        [SerializeField] private int mediumEntityCount = 7;
        [SerializeField] private int hardEntityCount = 9;

        [Header("Dodge Speed Settings")]
        [SerializeField] private float easySpeedMultiplier = 1.15f;
        [SerializeField] private float mediumSpeedMultiplier = 1.35f;
        [SerializeField] private float hardSpeedMultiplier = 1.55f;

        [Header("Dodge Spacing Settings")]
        [SerializeField] private float easyDodgeDelayMin = 1.15f;
        [SerializeField] private float easyDodgeDelayMax = 1.55f;
        [SerializeField] private float mediumDodgeDelayMin = 0.95f;
        [SerializeField] private float mediumDodgeDelayMax = 1.25f;
        [SerializeField] private float hardDodgeDelayMin = 0.75f;
        [SerializeField] private float hardDodgeDelayMax = 1.05f;

        [Header("Dodge Progression Per Wave")]
        [SerializeField] private int extraEntitiesEveryXWaves = 4;
        [SerializeField] private int maxExtraEntities = 3;
        [SerializeField] private float speedIncreasePerWave = 0.025f;
        [SerializeField] private float maxSpeedIncrease = 0.25f;
        [SerializeField] private float dodgeDelayDecreasePerWave = 0.015f;
        [SerializeField] private float minDodgeDelay = 0.65f;

        [Header("Background Enemy Spawn Settings")]
        [SerializeField] private float easyEnemySpawnInterval = 5.5f;
        [SerializeField] private float mediumEnemySpawnInterval = 4.0f;
        [SerializeField] private float hardEnemySpawnInterval = 3.0f;

        [Header("Background Enemy Limit Settings")]
        [SerializeField] private int easyMaxConcurrentEnemies = 1;
        [SerializeField] private int mediumMaxConcurrentEnemies = 2;
        [SerializeField] private int hardMaxConcurrentEnemies = 3;

        [Header("Background Enemy Speed Settings")]
        [SerializeField] private float easyEnemySpeedMultiplier = 0.95f;
        [SerializeField] private float mediumEnemySpeedMultiplier = 1.05f;
        [SerializeField] private float hardEnemySpeedMultiplier = 1.15f;

        [Header("Background Enemy Progression")]
        [SerializeField] private float enemySpawnIntervalDecreasePerWave = 0.06f;
        [SerializeField] private float minEnemySpawnInterval = 2.0f;
        [SerializeField] private float enemySpeedIncreasePerWave = 0.01f;
        [SerializeField] private float maxEnemySpeedIncrease = 0.15f;

        [Header("Boss Damage")]
        [SerializeField] private float bossDamagePerSuccessfulWave = 100f;

        // Backward compatible name for old code.
        public int GetEntityCount(int waveIndex)
        {
            return GetDodgeProjectileCount(waveIndex);
        }

        public int GetDodgeProjectileCount(int waveIndex)
        {
            int baseCount = GetBaseDodgeCountBySelectedDifficulty();
            int extra = GetExtraEntitiesByWaveIndex(waveIndex);

            return baseCount + extra;
        }

        public float GetSpeedMultiplier(int waveIndex)
        {
            float baseSpeed = GetBaseDodgeSpeedBySelectedDifficulty();
            float extraSpeed = Mathf.Min(waveIndex * speedIncreasePerWave, maxSpeedIncrease);

            return baseSpeed + extraSpeed;
        }

        public float GetRandomDodgeProjectileDelay(int waveIndex)
        {
            Vector2 delayRange = GetDodgeDelayRangeBySelectedDifficulty();
            float decrease = waveIndex * dodgeDelayDecreasePerWave;

            float minDelay = Mathf.Max(minDodgeDelay, delayRange.x - decrease);
            float maxDelay = Mathf.Max(minDelay, delayRange.y - decrease);

            return Random.Range(minDelay, maxDelay);
        }

        public float GetEnemySpawnInterval(int waveIndex)
        {
            float baseInterval = GetBaseEnemySpawnIntervalBySelectedDifficulty();
            float intervalDecrease = waveIndex * enemySpawnIntervalDecreasePerWave;

            return Mathf.Max(minEnemySpawnInterval, baseInterval - intervalDecrease);
        }

        public int GetMaxConcurrentEnemies()
        {
            switch (GameLaunchData.Difficulty)
            {
                case SelectedGameDifficulty.Easy:
                    return easyMaxConcurrentEnemies;

                case SelectedGameDifficulty.Medium:
                    return mediumMaxConcurrentEnemies;

                case SelectedGameDifficulty.Hard:
                    return hardMaxConcurrentEnemies;

                default:
                    return mediumMaxConcurrentEnemies;
            }
        }

        public float GetEnemySpeedMultiplier(int waveIndex)
        {
            float baseSpeed = GetBaseEnemySpeedBySelectedDifficulty();
            float extraSpeed = Mathf.Min(waveIndex * enemySpeedIncreasePerWave, maxEnemySpeedIncrease);

            return baseSpeed + extraSpeed;
        }

        public float GetBossDamagePerSuccessfulWave()
        {
            return bossDamagePerSuccessfulWave;
        }

        private int GetBaseDodgeCountBySelectedDifficulty()
        {
            switch (GameLaunchData.Difficulty)
            {
                case SelectedGameDifficulty.Easy:
                    return easyEntityCount;

                case SelectedGameDifficulty.Medium:
                    return mediumEntityCount;

                case SelectedGameDifficulty.Hard:
                    return hardEntityCount;

                default:
                    return mediumEntityCount;
            }
        }

        private float GetBaseDodgeSpeedBySelectedDifficulty()
        {
            switch (GameLaunchData.Difficulty)
            {
                case SelectedGameDifficulty.Easy:
                    return easySpeedMultiplier;

                case SelectedGameDifficulty.Medium:
                    return mediumSpeedMultiplier;

                case SelectedGameDifficulty.Hard:
                    return hardSpeedMultiplier;

                default:
                    return mediumSpeedMultiplier;
            }
        }

        private Vector2 GetDodgeDelayRangeBySelectedDifficulty()
        {
            switch (GameLaunchData.Difficulty)
            {
                case SelectedGameDifficulty.Easy:
                    return new Vector2(easyDodgeDelayMin, easyDodgeDelayMax);

                case SelectedGameDifficulty.Medium:
                    return new Vector2(mediumDodgeDelayMin, mediumDodgeDelayMax);

                case SelectedGameDifficulty.Hard:
                    return new Vector2(hardDodgeDelayMin, hardDodgeDelayMax);

                default:
                    return new Vector2(mediumDodgeDelayMin, mediumDodgeDelayMax);
            }
        }

        private float GetBaseEnemySpawnIntervalBySelectedDifficulty()
        {
            switch (GameLaunchData.Difficulty)
            {
                case SelectedGameDifficulty.Easy:
                    return easyEnemySpawnInterval;

                case SelectedGameDifficulty.Medium:
                    return mediumEnemySpawnInterval;

                case SelectedGameDifficulty.Hard:
                    return hardEnemySpawnInterval;

                default:
                    return mediumEnemySpawnInterval;
            }
        }

        private float GetBaseEnemySpeedBySelectedDifficulty()
        {
            switch (GameLaunchData.Difficulty)
            {
                case SelectedGameDifficulty.Easy:
                    return easyEnemySpeedMultiplier;

                case SelectedGameDifficulty.Medium:
                    return mediumEnemySpeedMultiplier;

                case SelectedGameDifficulty.Hard:
                    return hardEnemySpeedMultiplier;

                default:
                    return mediumEnemySpeedMultiplier;
            }
        }

        private int GetExtraEntitiesByWaveIndex(int waveIndex)
        {
            if (extraEntitiesEveryXWaves <= 0)
            {
                return 0;
            }

            int extra = waveIndex / extraEntitiesEveryXWaves;

            return Mathf.Clamp(extra, 0, maxExtraEntities);
        }

        private void OnValidate()
        {
            easyEntityCount = Mathf.Max(1, easyEntityCount);
            mediumEntityCount = Mathf.Max(1, mediumEntityCount);
            hardEntityCount = Mathf.Max(1, hardEntityCount);

            easySpeedMultiplier = Mathf.Max(0.1f, easySpeedMultiplier);
            mediumSpeedMultiplier = Mathf.Max(0.1f, mediumSpeedMultiplier);
            hardSpeedMultiplier = Mathf.Max(0.1f, hardSpeedMultiplier);

            easyDodgeDelayMin = Mathf.Max(0.1f, easyDodgeDelayMin);
            easyDodgeDelayMax = Mathf.Max(easyDodgeDelayMin, easyDodgeDelayMax);
            mediumDodgeDelayMin = Mathf.Max(0.1f, mediumDodgeDelayMin);
            mediumDodgeDelayMax = Mathf.Max(mediumDodgeDelayMin, mediumDodgeDelayMax);
            hardDodgeDelayMin = Mathf.Max(0.1f, hardDodgeDelayMin);
            hardDodgeDelayMax = Mathf.Max(hardDodgeDelayMin, hardDodgeDelayMax);

            maxExtraEntities = Mathf.Max(0, maxExtraEntities);
            speedIncreasePerWave = Mathf.Max(0f, speedIncreasePerWave);
            maxSpeedIncrease = Mathf.Max(0f, maxSpeedIncrease);
            dodgeDelayDecreasePerWave = Mathf.Max(0f, dodgeDelayDecreasePerWave);
            minDodgeDelay = Mathf.Max(0.1f, minDodgeDelay);

            easyEnemySpawnInterval = Mathf.Max(0.5f, easyEnemySpawnInterval);
            mediumEnemySpawnInterval = Mathf.Max(0.5f, mediumEnemySpawnInterval);
            hardEnemySpawnInterval = Mathf.Max(0.5f, hardEnemySpawnInterval);

            easyMaxConcurrentEnemies = Mathf.Max(0, easyMaxConcurrentEnemies);
            mediumMaxConcurrentEnemies = Mathf.Max(0, mediumMaxConcurrentEnemies);
            hardMaxConcurrentEnemies = Mathf.Max(0, hardMaxConcurrentEnemies);

            easyEnemySpeedMultiplier = Mathf.Max(0.1f, easyEnemySpeedMultiplier);
            mediumEnemySpeedMultiplier = Mathf.Max(0.1f, mediumEnemySpeedMultiplier);
            hardEnemySpeedMultiplier = Mathf.Max(0.1f, hardEnemySpeedMultiplier);

            enemySpawnIntervalDecreasePerWave = Mathf.Max(0f, enemySpawnIntervalDecreasePerWave);
            minEnemySpawnInterval = Mathf.Max(0.5f, minEnemySpawnInterval);
            enemySpeedIncreasePerWave = Mathf.Max(0f, enemySpeedIncreasePerWave);
            maxEnemySpeedIncrease = Mathf.Max(0f, maxEnemySpeedIncrease);
            bossDamagePerSuccessfulWave = Mathf.Max(0f, bossDamagePerSuccessfulWave);
        }
    }
}
