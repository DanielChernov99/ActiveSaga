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
                ? difficultyConfig.GetEntityCount(index)
                : 7;

            dynamicWave.steps.Add(new WaveStep
            {
                type = WaveStep.StepType.BossAnimation,
                animationTrigger = "Attack",
                delayAfterStep = 1f
            });

            if (type == WaveType.Combat)
            {
                AddEnemySteps(dynamicWave, entityCount);
            }
            else
            {
                AddProjectileSteps(dynamicWave, entityCount);
            }

            return dynamicWave;
        }

        private void AddEnemySteps(WaveData dynamicWave, int enemyCount)
        {
            for (int i = 0; i < enemyCount; i++)
            {
                dynamicWave.steps.Add(new WaveStep
                {
                    type = WaveStep.StepType.SpawnEnemy,
                    enemyData = GetRandomEnemyData(),
                    spawnOffset = new Vector3(Random.Range(-3f, 3f), 0, Random.Range(4f, 8f)),
                    delayAfterStep = Random.Range(0.5f, 1.5f)
                });
            }
        }

        private void AddProjectileSteps(WaveData dynamicWave, int projectileCount)
        {
            for (int i = 0; i < projectileCount; i++)
            {
                dynamicWave.steps.Add(new WaveStep
                {
                    type = WaveStep.StepType.SpawnProjectile,
                    projectileData = GetRandomProjectileData(),
                    spawnOffset = new Vector3(Random.Range(-2.2f, 2.2f), 0f, Random.Range(3f, 6f)),
                    delayAfterStep = Random.Range(0.9f, 1.6f)
                });
            }
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