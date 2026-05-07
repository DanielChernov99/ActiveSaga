using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ActiveSaga.BossFight.Core;
using ActiveSaga.BossFight.Data;
using ActiveSaga.BossFight.Entities;

namespace ActiveSaga.BossFight.Waves
{
    public class WaveManager : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private List<WaveData> waveConfigs;
        [SerializeField] private DifficultyConfig difficultyConfig;

        [Header("Settings")]
        [SerializeField] private Transform bossSpawnPoint;

        [Header("Runtime State")]
        [SerializeField] private int currentWaveIndex = 0;
        private int _activeEntitiesCount = 0;
        private bool _isWaveActive = false;

        private void OnEnable()
        {
            EventManager.Subscribe<EnemySpawnedEvent>(OnEntitySpawned);
            EventManager.Subscribe<EnemyDespawnedEvent>(OnEntityDespawned);
            EventManager.Subscribe<ProjectileSpawnedEvent>(OnEntitySpawned);
            EventManager.Subscribe<ProjectileDespawnedEvent>(OnEntityDespawned);
        }

        private void OnDisable()
        {
            EventManager.Unsubscribe<EnemySpawnedEvent>(OnEntitySpawned);
            EventManager.Unsubscribe<EnemyDespawnedEvent>(OnEntityDespawned);
            EventManager.Unsubscribe<ProjectileSpawnedEvent>(OnEntitySpawned);
            EventManager.Unsubscribe<ProjectileDespawnedEvent>(OnEntityDespawned);
        }

        private void Start()
        {
            if (waveConfigs == null || waveConfigs.Count == 0) return;
            
            if (bossSpawnPoint == null)
            {
                var boss = BossController.Instance;
                if (boss != null) bossSpawnPoint = boss.transform;
            }

            StartCoroutine(WaveLoopRoutine());
        }

        private IEnumerator WaveLoopRoutine()
        {
            yield return new WaitForSeconds(3f); 

            while (currentWaveIndex < waveConfigs.Count)
            {
                yield return StartCoroutine(PlayWave(waveConfigs[currentWaveIndex]));
                currentWaveIndex++;
                yield return new WaitForSeconds(3f); 
            }

            EventManager.Trigger(new FeedbackEvent { message = "Victory!", duration = 10f });
        }

        private IEnumerator PlayWave(WaveData data)
        {
            _isWaveActive = true;
            Debug.Log($"<color=cyan>Starting Wave {currentWaveIndex + 1}: {data.waveName}</color>");
            EventManager.Trigger(new WaveStartedEvent { waveIndex = currentWaveIndex + 1, name = data.waveName });

            float speedMult = difficultyConfig != null ? difficultyConfig.GetSpeedMultiplier(currentWaveIndex) : 1f;

            foreach (var step in data.steps)
            {
                ExecuteStep(step, speedMult);
                yield return new WaitForSeconds(step.delayAfterStep / speedMult);
            }

            // Wait for all spawned entities to be cleared
            if (_activeEntitiesCount > 0)
            {
                Debug.Log($"Steps finished. Waiting for {_activeEntitiesCount} active entities...");
                yield return new WaitUntil(() => _activeEntitiesCount <= 0);
            }

            Debug.Log($"Wave {currentWaveIndex + 1} Cleared.");
            EventManager.Trigger(new WaveCompletedEvent { success = true });
            _isWaveActive = false;
        }

        private void ExecuteStep(WaveStep step, float speedMultiplier)
        {
            switch (step.type)
            {
                case WaveStep.StepType.SpawnEnemy:
                    SpawnEnemy(step.enemyData, step.spawnOffset);
                    break;
                case WaveStep.StepType.SpawnProjectile:
                    SpawnProjectile(step.projectileData, step.spawnOffset);
                    break;
                case WaveStep.StepType.BossAnimation:
                    if (BossController.Instance != null) BossController.Instance.PlayAnimation(step.animationTrigger);
                    break;
            }
        }

        private void SpawnEnemy(EnemyData data, Vector3 offset)
        {
            if (data == null) return;
            
            Vector3 basePos = bossSpawnPoint != null ? bossSpawnPoint.position : Vector3.zero;
            Vector3 spawnPos = basePos + offset;

            GameObject enemy = PoolManager.Instance.SpawnFromPool(data.enemyName, spawnPos, Quaternion.identity, true);
            if (enemy != null)
            {
                var controller = enemy.GetComponent<EnemyController>();
                if (controller != null) controller.Initialize(data);
            }
        }

        private void SpawnProjectile(ProjectileData data, Vector3 offset)
        {
            if (data == null) return;
            
            Vector3 basePos = bossSpawnPoint != null ? bossSpawnPoint.position : Vector3.zero;
            Vector3 spawnPos = basePos + offset;

            GameObject projectile = PoolManager.Instance.SpawnFromPool(data.projectileName, spawnPos, Quaternion.identity, false);
            if (projectile != null)
            {
                var controller = projectile.GetComponent<ProjectileController>();
                if (controller != null) controller.Initialize(data);
            }
        }

        private void OnEntitySpawned(EnemySpawnedEvent e) { _activeEntitiesCount++; }
        private void OnEntitySpawned(ProjectileSpawnedEvent e) { _activeEntitiesCount++; }
        private void OnEntityDespawned(EnemyDespawnedEvent e) { _activeEntitiesCount--; }
        private void OnEntityDespawned(ProjectileDespawnedEvent e) { _activeEntitiesCount--; }

        private void OnDrawGizmos()
        {
            if (bossSpawnPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(bossSpawnPoint.position, 1f);
            }
        }
    }
}

