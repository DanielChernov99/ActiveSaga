using UnityEngine;
using System.Collections;

namespace ActiveSaga.BossFight
{
    public class DodgeWave : BossWave
    {
        [Header("References")]
        [SerializeField] private GameObject obstaclePrefab;
        [SerializeField] private Transform[] spawnPoints;
        
        [Header("Settings")]
        [SerializeField] private int obstacleCount = 10;
        [SerializeField] private float spawnInterval = 1.5f;

        private Transform playerCamera;
        private BossFightManager manager;
        private bool isRunning = false;

        public override void StartWave(float difficultyMultiplier)
        {
            if (isRunning) return;
            
            manager = FindFirstObjectByType<BossFightManager>();
            Camera mainCam = Camera.main;
            if (mainCam != null) playerCamera = mainCam.transform;

            StartCoroutine(SpawnRoutine(difficultyMultiplier));
        }

        public override void EndWave()
        {
            isRunning = false;
            Debug.Log("Dodge Wave Logic Ended.");
        }

        private IEnumerator SpawnRoutine(float difficulty)
        {
            isRunning = true;
            int spawned = 0;
            int total = Mathf.RoundToInt(obstacleCount * difficulty);
            
            // Update base class total targets for completion check
            // Note: In Step 2 we added ReportSuccess/Failure to manager.
            // Manager handles the counts.

            while (spawned < total)
            {
                SpawnObstacle();
                spawned++;
                yield return new WaitForSeconds(spawnInterval / difficulty);
            }

            // Wait until all projectiles are destroyed
            yield return new WaitUntil(() => GameObject.FindObjectsByType<ObstacleProjectile>(FindObjectsSortMode.None).Length == 0);
            
            manager.EndCurrentWave();
        }

        private void SpawnObstacle()
        {
            if (obstaclePrefab == null || playerCamera == null) return;

            // Pick a random spawn point or generate one in front of player
            Vector3 spawnPos;
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                spawnPos = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
            }
            else
            {
                // Fallback: Spawn 10 meters in front of player with some variance
                spawnPos = playerCamera.position + playerCamera.forward * 10f + playerCamera.right * Random.Range(-2f, 2f) + playerCamera.up * Random.Range(-0.5f, 1f);
            }

            GameObject obj = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
            var projectile = obj.GetComponent<ObstacleProjectile>();
            if (projectile == null) projectile = obj.AddComponent<ObstacleProjectile>();
            
            projectile.Initialize(playerCamera.position, 5f, manager);
        }
    }
}
