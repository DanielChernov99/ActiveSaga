using System.Collections.Generic;
using UnityEngine;
using ActiveSaga.BossFight.Data;

namespace ActiveSaga.BossFight.Waves
{
    public class DynamicWaveGenerator
    {
        private readonly List<WaveData> waveConfigs;
        private readonly DifficultyConfig difficultyConfig;
        private readonly List<EnemyData> enemyMasterList;
        private readonly List<ProjectileData> projectileMasterList;

        public DynamicWaveGenerator(
            List<WaveData> waveConfigs,
            DifficultyConfig difficultyConfig,
            List<EnemyData> enemyMasterList,
            List<ProjectileData> projectileMasterList)
        {
            this.waveConfigs = waveConfigs;
            this.difficultyConfig = difficultyConfig;
            this.enemyMasterList = enemyMasterList;
            this.projectileMasterList = projectileMasterList;
        }

        public WaveData Generate(int index, WaveType type)
        {
            WaveData dynamicWave = ScriptableObject.CreateInstance<WaveData>();
            dynamicWave.waveName = $"Dynamic {type} Wave {index + 1}";
            dynamicWave.waveType = type;
            dynamicWave.steps = new List<WaveStep>();

            int entityCount = difficultyConfig != null
                ? difficultyConfig.GetDodgeProjectileCount(index)
                : 7;

            dynamicWave.steps.Add(new WaveStep
            {
                type = WaveStep.StepType.BossAnimation,
                animationTrigger = "Attack",
                delayAfterStep = 1f
            });

            // The main Fight Game flow is now always dodge waves.
            // Combat enemies are spawned separately by WaveManager as a background system.
            if (type == WaveType.Combat)
            {
                AddEnemySteps(dynamicWave, entityCount);
            }
            else
            {
                AddProjectileSteps(dynamicWave, entityCount, index);
            }

            return dynamicWave;
        }

        public WaveStep GenerateBackgroundEnemyStep(int waveIndex)
        {
            return new WaveStep
            {
                type = WaveStep.StepType.SpawnEnemy,
                enemyData = GetRandomEnemyData(),
                spawnOffset = GetBackgroundEnemySpawnOffset(waveIndex),
                delayAfterStep = 0f
            };
        }

        private void AddEnemySteps(WaveData dynamicWave, int enemyCount)
        {
            for (int i = 0; i < enemyCount; i++)
            {
                dynamicWave.steps.Add(new WaveStep
                {
                    type = WaveStep.StepType.SpawnEnemy,
                    enemyData = GetRandomEnemyData(),
                    spawnOffset = GetFrontEnemySpawnOffset(),
                    delayAfterStep = Random.Range(0.8f, 1.4f)
                });
            }
        }

        private void AddProjectileSteps(WaveData dynamicWave, int projectileCount, int waveIndex)
        {
            for (int i = 0; i < projectileCount; i++)
            {
                dynamicWave.steps.Add(new WaveStep
                {
                    type = WaveStep.StepType.SpawnProjectile,
                    projectileData = GetRandomProjectileData(),
                    spawnOffset = GetProjectileSpawnOffset(i),
                    delayAfterStep = difficultyConfig != null
                        ? difficultyConfig.GetRandomDodgeProjectileDelay(waveIndex)
                        : Random.Range(0.9f, 1.3f)
                });
            }
        }

        private Vector3 GetProjectileSpawnOffset(int projectileIndex)
        {
            // Keep the obstacle in a playable lane, but avoid sending everything exactly through the center.
            // The Z range keeps enough distance for jump/duck reactions while the speed is higher.
            float x = Random.Range(-1.9f, 1.9f);
            float z = Random.Range(4.5f, 7.0f);

            return new Vector3(x, 0f, z);
        }

        private Vector3 GetBackgroundEnemySpawnOffset(int waveIndex)
        {
            // Most enemies now enter from the sides so they feel like an additional threat
            // and not like another front wave.
            bool spawnFromSide = Random.value < 0.7f;

            if (spawnFromSide)
            {
                float side = Random.value < 0.5f ? -1f : 1f;
                float sideDistance = Random.Range(5.5f, 8.5f) * side;
                float forwardDistance = Random.Range(1.5f, 5.0f);

                return new Vector3(sideDistance, 0f, forwardDistance);
            }

            return GetFrontEnemySpawnOffset();
        }

        private Vector3 GetFrontEnemySpawnOffset()
        {
            return new Vector3(Random.Range(-2.8f, 2.8f), 0f, Random.Range(4.0f, 8.0f));
        }

        private EnemyData GetRandomEnemyData()
        {
            if (enemyMasterList != null && enemyMasterList.Count > 0)
            {
                return enemyMasterList[Random.Range(0, enemyMasterList.Count)];
            }

            if (waveConfigs != null)
            {
                foreach (WaveData config in waveConfigs)
                {
                    if (config == null || config.steps == null)
                    {
                        continue;
                    }

                    WaveStep step = config.steps.Find(s => s.enemyData != null);

                    if (step != null)
                    {
                        return step.enemyData;
                    }
                }
            }

            return null;
        }

        private ProjectileData GetRandomProjectileData()
        {
            if (projectileMasterList != null && projectileMasterList.Count > 0)
            {
                return projectileMasterList[Random.Range(0, projectileMasterList.Count)];
            }

            if (waveConfigs != null)
            {
                foreach (WaveData config in waveConfigs)
                {
                    if (config == null || config.steps == null)
                    {
                        continue;
                    }

                    WaveStep step = config.steps.Find(s => s.projectileData != null);

                    if (step != null)
                    {
                        return step.projectileData;
                    }
                }
            }

            return null;
        }
    }
}
