using System;
using System.Collections.Generic;
using UnityEngine;
using ActiveSaga.BossFight.Core;
using ActiveSaga.BossFight.Entities;

namespace ActiveSaga.BossFight.Waves
{
    public class WaveEntityTracker
    {
        private readonly HashSet<GameObject> activeWaveProjectiles = new HashSet<GameObject>();
        private readonly HashSet<GameObject> activeBackgroundEnemies = new HashSet<GameObject>();
        private readonly Func<bool> canCountStats;

        private bool isSubscribed;

        // This count is used by WaveManager to know when a dodge wave is finished.
        public int ActiveEntitiesCount => activeWaveProjectiles.Count;

        // This count is used only by the background enemy spawner to avoid enemy spam.
        public int ActiveEnemiesCount => activeBackgroundEnemies.Count;

        public int TotalSpawnedThisWave { get; private set; }
        public int SuccessfullyHandledThisWave { get; private set; }
        public int PlayerHitCountThisWave { get; private set; }

        public WaveEntityTracker(Func<bool> canCountStats)
        {
            this.canCountStats = canCountStats;
        }

        public void Subscribe()
        {
            if (isSubscribed)
            {
                return;
            }

            EventManager.Subscribe<EnemySpawnedEvent>(OnEnemySpawned);
            EventManager.Subscribe<EnemyDespawnedEvent>(OnEnemyDespawned);
            EventManager.Subscribe<ProjectileSpawnedEvent>(OnProjectileSpawned);
            EventManager.Subscribe<ProjectileDespawnedEvent>(OnProjectileDespawned);

            isSubscribed = true;
        }

        public void Unsubscribe()
        {
            if (!isSubscribed)
            {
                return;
            }

            EventManager.Unsubscribe<EnemySpawnedEvent>(OnEnemySpawned);
            EventManager.Unsubscribe<EnemyDespawnedEvent>(OnEnemyDespawned);
            EventManager.Unsubscribe<ProjectileSpawnedEvent>(OnProjectileSpawned);
            EventManager.Unsubscribe<ProjectileDespawnedEvent>(OnProjectileDespawned);

            isSubscribed = false;
        }

        public void ResetWaveCounters()
        {
            TotalSpawnedThisWave = 0;
            SuccessfullyHandledThisWave = 0;
            PlayerHitCountThisWave = 0;
        }

        public void CleanupInactiveEntities()
        {
            activeWaveProjectiles.RemoveWhere(item => item == null || !item.activeInHierarchy);
            activeBackgroundEnemies.RemoveWhere(item => item == null || !item.activeInHierarchy);
        }

        public void ForceClearActiveEntities()
        {
            Debug.Log($"<color=orange>Force clearing {activeWaveProjectiles.Count} active wave projectiles.</color>");

            List<GameObject> toClear = new List<GameObject>(activeWaveProjectiles);

            foreach (GameObject obj in toClear)
            {
                if (obj == null)
                {
                    continue;
                }

                ProjectileController projectile = obj.GetComponent<ProjectileController>();

                if (projectile != null)
                {
                    projectile.Despawn();
                    continue;
                }

                obj.SetActive(false);
            }

            activeWaveProjectiles.Clear();
        }

        public void ForceClearActiveEntitiesWithoutStats()
        {
            int totalCount = activeWaveProjectiles.Count + activeBackgroundEnemies.Count;

            if (totalCount == 0)
            {
                return;
            }

            Debug.Log($"<color=orange>[WaveManager] Force clearing {totalCount} entities without adding stats.</color>");

            List<GameObject> toClear = new List<GameObject>();
            toClear.AddRange(activeWaveProjectiles);
            toClear.AddRange(activeBackgroundEnemies);

            foreach (GameObject obj in toClear)
            {
                if (obj == null)
                {
                    continue;
                }

                obj.SetActive(false);
            }

            activeWaveProjectiles.Clear();
            activeBackgroundEnemies.Clear();
        }

        private void OnEnemySpawned(EnemySpawnedEvent e)
        {
            if (e.enemy != null && !activeBackgroundEnemies.Contains(e.enemy))
            {
                activeBackgroundEnemies.Add(e.enemy);
            }
        }

        private void OnProjectileSpawned(ProjectileSpawnedEvent e)
        {
            if (!CanCountStats())
            {
                return;
            }

            if (e.projectile != null && !activeWaveProjectiles.Contains(e.projectile))
            {
                activeWaveProjectiles.Add(e.projectile);
                TotalSpawnedThisWave++;
            }
        }

        private void OnEnemyDespawned(EnemyDespawnedEvent e)
        {
            if (e.enemy == null)
            {
                return;
            }

            activeBackgroundEnemies.Remove(e.enemy);
        }

        private void OnProjectileDespawned(ProjectileDespawnedEvent e)
        {
            if (e.projectile == null)
            {
                return;
            }

            activeWaveProjectiles.Remove(e.projectile);

            if (!CanCountStats())
            {
                return;
            }

            if (e.wasDodged)
            {
                SuccessfullyHandledThisWave++;
            }
            else if (e.wasHitPlayer)
            {
                PlayerHitCountThisWave++;
            }
        }

        private bool CanCountStats()
        {
            return canCountStats == null || canCountStats.Invoke();
        }
    }
}
