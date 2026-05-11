using System;
using System.Collections.Generic;
using UnityEngine;
using ActiveSaga.BossFight.Core;
using ActiveSaga.BossFight.Entities;

namespace ActiveSaga.BossFight.Waves
{
    public class WaveEntityTracker
    {
        private readonly HashSet<GameObject> activeEntities = new HashSet<GameObject>();
        private readonly Func<bool> canCountStats;

        private bool isSubscribed;

        public int ActiveEntitiesCount => activeEntities.Count;
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

            EventManager.Subscribe<EnemySpawnedEvent>(OnEntitySpawned);
            EventManager.Subscribe<EnemyDespawnedEvent>(OnEnemyDespawned);
            EventManager.Subscribe<ProjectileSpawnedEvent>(OnEntitySpawned);
            EventManager.Subscribe<ProjectileDespawnedEvent>(OnProjectileDespawned);

            isSubscribed = true;
        }

        public void Unsubscribe()
        {
            if (!isSubscribed)
            {
                return;
            }

            EventManager.Unsubscribe<EnemySpawnedEvent>(OnEntitySpawned);
            EventManager.Unsubscribe<EnemyDespawnedEvent>(OnEnemyDespawned);
            EventManager.Unsubscribe<ProjectileSpawnedEvent>(OnEntitySpawned);
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
            if (activeEntities.Count == 0)
            {
                return;
            }

            activeEntities.RemoveWhere(item => item == null || !item.activeInHierarchy);
        }

        public void ForceClearActiveEntities()
        {
            Debug.Log($"<color=orange>Force clearing {activeEntities.Count} entities.</color>");

            List<GameObject> toClear = new List<GameObject>(activeEntities);

            foreach (GameObject obj in toClear)
            {
                if (obj == null)
                {
                    continue;
                }

                EnemyController enemy = obj.GetComponent<EnemyController>();

                if (enemy != null)
                {
                    enemy.Despawn(false);
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

            activeEntities.Clear();
        }

        public void ForceClearActiveEntitiesWithoutStats()
        {
            if (activeEntities.Count == 0)
            {
                return;
            }

            Debug.Log($"<color=orange>[WaveManager] Force clearing {activeEntities.Count} entities without adding stats.</color>");

            List<GameObject> toClear = new List<GameObject>(activeEntities);

            foreach (GameObject obj in toClear)
            {
                if (obj == null)
                {
                    continue;
                }

                obj.SetActive(false);
            }

            activeEntities.Clear();
        }

        private void OnEntitySpawned(EnemySpawnedEvent e)
        {
            if (!CanCountStats())
            {
                return;
            }

            if (e.enemy != null && !activeEntities.Contains(e.enemy))
            {
                activeEntities.Add(e.enemy);
                TotalSpawnedThisWave++;
            }
        }

        private void OnEntitySpawned(ProjectileSpawnedEvent e)
        {
            if (!CanCountStats())
            {
                return;
            }

            if (e.projectile != null && !activeEntities.Contains(e.projectile))
            {
                activeEntities.Add(e.projectile);
                TotalSpawnedThisWave++;
            }
        }

        private void OnEnemyDespawned(EnemyDespawnedEvent e)
        {
            if (e.enemy == null)
            {
                return;
            }

            activeEntities.Remove(e.enemy);

            if (!CanCountStats())
            {
                return;
            }

            if (e.wasKilledByPlayer)
            {
                SuccessfullyHandledThisWave++;
            }
            else
            {
                PlayerHitCountThisWave++;
            }
        }

        private void OnProjectileDespawned(ProjectileDespawnedEvent e)
        {
            if (e.projectile == null)
            {
                return;
            }

            activeEntities.Remove(e.projectile);

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