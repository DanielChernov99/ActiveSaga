using System;
using UnityEngine;
using ActiveSaga.BossFight.Core;
using ActiveSaga.BossFight.Data;
using ActiveSaga.BossFight.Entities;

namespace ActiveSaga.BossFight.Waves
{
    public class WaveEntitySpawner
    {
        private Transform bossSpawnPoint;
        private readonly Func<bool> canSpawn;

        public WaveEntitySpawner(Transform bossSpawnPoint, Func<bool> canSpawn)
        {
            this.bossSpawnPoint = bossSpawnPoint;
            this.canSpawn = canSpawn;
        }

        public void SetBossSpawnPoint(Transform newBossSpawnPoint)
        {
            bossSpawnPoint = newBossSpawnPoint;
        }

        public void ExecuteStep(WaveStep step, float speedMultiplier)
        {
            if (step == null)
            {
                return;
            }

            if (!CanSpawn())
            {
                return;
            }

            switch (step.type)
            {
                case WaveStep.StepType.SpawnEnemy:
                    SpawnEnemy(step.enemyData, step.spawnOffset, speedMultiplier);
                    break;

                case WaveStep.StepType.SpawnProjectile:
                    SpawnProjectile(step.projectileData, step.spawnOffset, speedMultiplier);
                    break;

                case WaveStep.StepType.BossAnimation:
                    if (BossController.Instance != null)
                    {
                        BossController.Instance.PlayAnimation(step.animationTrigger);
                    }
                    break;
            }
        }

        private void SpawnEnemy(EnemyData data, Vector3 offset, float speedMultiplier)
        {
            if (!CanSpawn())
            {
                return;
            }

            if (data == null || PoolManager.Instance == null || bossSpawnPoint == null)
            {
                return;
            }

            Vector3 basePos = bossSpawnPoint.position;
            Vector3 forward = Vector3.forward;

            if (BossFightGameManager.Instance != null &&
                BossFightGameManager.Instance.PlayerTransform != null)
            {
                forward = (BossFightGameManager.Instance.PlayerTransform.position - basePos).normalized;
                forward.y = 0;
            }

            Vector3 right = Vector3.Cross(Vector3.up, forward);

            Vector3 spawnPos =
                basePos +
                forward * offset.z +
                right * offset.x +
                Vector3.up * offset.y;

            Quaternion spawnRot = Quaternion.LookRotation(forward);

            GameObject enemy = PoolManager.Instance.SpawnFromPool(
                data.enemyName,
                spawnPos,
                spawnRot,
                true
            );

            if (enemy != null)
            {
                EnemyController controller = enemy.GetComponent<EnemyController>();

                if (controller != null)
                {
                    controller.Initialize(data, speedMultiplier);
                }
                else
                {
                    Debug.LogError($"Missing EnemyController on {enemy.name}");
                }
            }
        }

        private void SpawnProjectile(ProjectileData data, Vector3 offset, float speedMultiplier)
        {
            if (!CanSpawn())
            {
                return;
            }

            if (data == null)
            {
                Debug.LogError("SpawnProjectile FAILED: ProjectileData is NULL!");
                return;
            }

            if (PoolManager.Instance == null || bossSpawnPoint == null)
            {
                return;
            }

            Vector3 basePos = bossSpawnPoint.position;
            bool hasPlayer = BossFightGameManager.Instance?.PlayerTransform != null;

            Vector3 playerPos = hasPlayer
                ? BossFightGameManager.Instance.PlayerTransform.position
                : basePos + Vector3.forward * 5f;

            bool isHeadTarget = UnityEngine.Random.value > 0.5f;

            float targetHeight = isHeadTarget ? 3.0f : 0.9f;

            float floorY = hasPlayer
                ? BossFightGameManager.Instance.PlayerTransform.position.y
                : basePos.y;

            Vector3 targetPos = playerPos;
            targetPos.y = floorY + targetHeight;

            Vector3 spawnStartPos = basePos;
            spawnStartPos.y = floorY + targetHeight;

            Vector3 direction = (targetPos - spawnStartPos).normalized;

            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Vector3.forward;
            }

            Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;

            if (right.sqrMagnitude < 0.001f)
            {
                right = Vector3.right;
            }

            Vector3 spawnPos =
                spawnStartPos +
                direction * offset.z +
                right * offset.x;

            Quaternion spawnRot = Quaternion.LookRotation(direction);

            GameObject projectile = PoolManager.Instance.SpawnFromPool(
                data.projectileName,
                spawnPos,
                spawnRot,
                false
            );

            if (projectile == null)
            {
                Debug.LogError($"Pool returned NULL for {data.projectileName}");
                return;
            }

            ProjectileController controller = projectile.GetComponent<ProjectileController>();

            if (controller == null)
            {
                Debug.LogError($"Missing ProjectileController on {projectile.name}");
                return;
            }

            controller.Initialize(data, speedMultiplier);
        }

        private bool CanSpawn()
        {
            return canSpawn == null || canSpawn.Invoke();
        }
    }
}