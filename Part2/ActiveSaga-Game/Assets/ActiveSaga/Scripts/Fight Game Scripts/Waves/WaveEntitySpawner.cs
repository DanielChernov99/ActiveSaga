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
                forward = BossFightGameManager.Instance.PlayerTransform.position - basePos;
                forward.y = 0f;
                forward.Normalize();
            }

            if (forward.sqrMagnitude < 0.001f)
            {
                forward = Vector3.forward;
            }

            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            if (right.sqrMagnitude < 0.001f)
            {
                right = Vector3.right;
            }

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
            Vector3 playerPos = GetPlayerAimPosition(basePos);
            float floorY = GetPlayerFloorY(basePos);

            ProjectileDodgeAction dodgeAction = data.ResolveDodgeAction();
            float targetHeight = data.GetTargetHeight(dodgeAction) + offset.y;

            Vector3 targetPos = playerPos;
            targetPos.y = floorY + targetHeight;

            Vector3 spawnStartPos = basePos;
            spawnStartPos.y = floorY + targetHeight;

            Vector3 direction = targetPos - spawnStartPos;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Vector3.forward;
            }
            else
            {
                direction.Normalize();
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

        private Vector3 GetPlayerAimPosition(Vector3 fallbackBasePosition)
        {
            if (BossFightGameManager.Instance != null)
            {
                if (BossFightGameManager.Instance.PlayerCamera != null)
                {
                    return BossFightGameManager.Instance.PlayerCamera.transform.position;
                }

                if (Camera.main != null)
                {
                    return Camera.main.transform.position;
                }

                if (BossFightGameManager.Instance.PlayerTransform != null)
                {
                    return BossFightGameManager.Instance.PlayerTransform.position;
                }
            }

            return fallbackBasePosition + Vector3.forward * 5f;
        }

        private float GetPlayerFloorY(Vector3 fallbackBasePosition)
        {
            if (BossFightGameManager.Instance != null &&
                BossFightGameManager.Instance.PlayerTransform != null)
            {
                return BossFightGameManager.Instance.PlayerTransform.position.y;
            }

            if (BossFightGameManager.Instance != null &&
                BossFightGameManager.Instance.PlayerCamera != null)
            {
                return BossFightGameManager.Instance.PlayerCamera.transform.position.y - 1.6f;
            }

            if (Camera.main != null)
            {
                return Camera.main.transform.position.y - 1.6f;
            }

            return fallbackBasePosition.y;
        }

        private bool CanSpawn()
        {
            return canSpawn == null || canSpawn.Invoke();
        }
    }
}