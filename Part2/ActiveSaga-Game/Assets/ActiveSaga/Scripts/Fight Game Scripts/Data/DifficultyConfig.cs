using UnityEngine;
using ActiveSaga.Common.GameSession;

namespace ActiveSaga.BossFight.Data
{
    [CreateAssetMenu(fileName = "DifficultyConfig", menuName = "BossFight/DifficultyConfig")]
    public class DifficultyConfig : ScriptableObject
    {
        [Header("Wave Count Settings")]
        [SerializeField] private int easyEntityCount = 4;
        [SerializeField] private int mediumEntityCount = 7;
        [SerializeField] private int hardEntityCount = 10;

        [Header("Speed Settings")]
        [SerializeField] private float easySpeedMultiplier = 0.85f;
        [SerializeField] private float mediumSpeedMultiplier = 1.0f;
        [SerializeField] private float hardSpeedMultiplier = 1.2f;

        [Header("Progression Per Wave")]
        [SerializeField] private int extraEntitiesEveryXWaves = 3;
        [SerializeField] private int maxExtraEntities = 3;
        [SerializeField] private float speedIncreasePerWave = 0.03f;
        [SerializeField] private float maxSpeedIncrease = 0.35f;

        [Header("Boss Damage")]
        [SerializeField] private float bossDamagePerSuccessfulWave = 100f;

        public int GetEntityCount(int waveIndex)
        {
            int baseCount = GetBaseEntityCountBySelectedDifficulty();
            int extra = GetExtraEntitiesByWaveIndex(waveIndex);

            return baseCount + extra;
        }

        public float GetSpeedMultiplier(int waveIndex)
        {
            float baseSpeed = GetBaseSpeedBySelectedDifficulty();
            float extraSpeed = Mathf.Min(waveIndex * speedIncreasePerWave, maxSpeedIncrease);

            return baseSpeed + extraSpeed;
        }

        public float GetBossDamagePerSuccessfulWave()
        {
            return bossDamagePerSuccessfulWave;
        }

        private int GetBaseEntityCountBySelectedDifficulty()
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

        private float GetBaseSpeedBySelectedDifficulty()
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

        private int GetExtraEntitiesByWaveIndex(int waveIndex)
        {
            if (extraEntitiesEveryXWaves <= 0)
            {
                return 0;
            }

            int extra = waveIndex / extraEntitiesEveryXWaves;

            return Mathf.Clamp(extra, 0, maxExtraEntities);
        }
    }
}