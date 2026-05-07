using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ActiveSaga.BossFight
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject redSkeletonPrefab;
        [SerializeField] private GameObject blueSkeletonPrefab;
        [SerializeField] private BossFightManager manager;
        [SerializeField] private Transform spawnCenter;

        [Header("Wave Configuration")]
        [SerializeField] private int enemiesPerWave = 10;
        [SerializeField] private float spawnInterval = 2f;
        [SerializeField] private float spawnRadius = 5f;

        private Transform playerCamera;
        private bool isSpawning = false;

        private void Start()
        {
            if (manager == null) manager = FindObjectOfType<BossFightManager>();
            
            // Find XR Origin Main Camera
            Camera mainCam = Camera.main;
            if (mainCam != null) playerCamera = mainCam.transform;
            
            if (spawnCenter == null) spawnCenter = transform;
        }

        public void StartWave(float difficulty)
        {
            if (isSpawning) return;
            StartCoroutine(SpawnWaveRoutine(difficulty));
        }

        private IEnumerator SpawnWaveRoutine(float difficulty)
        {
            isSpawning = true;
            int spawnedCount = 0;
            int totalToSpawn = Mathf.RoundToInt(enemiesPerWave * difficulty);

            Debug.Log($"Starting Enemy Spawner Wave: {totalToSpawn} enemies.");

            while (spawnedCount < totalToSpawn)
            {
                SpawnEnemy();
                spawnedCount++;
                yield return new WaitForSeconds(spawnInterval / difficulty);
            }

            isSpawning = false;
            
            // Wait for all enemies to be destroyed before finishing the wave
            // In a real scenario, we might track active enemies.
            // For now, let's wait a fixed time or check if any skeletons exist.
            yield return new WaitUntil(() => GameObject.FindObjectsByType<SkeletonEnemy>(FindObjectsSortMode.None).Length == 0);

            manager.EndCurrentWave();
        }

        private void SpawnEnemy()
        {
            // Requirement: random X between -5 and 5, random Z between 15 and 25
            // We'll calculate this relative to the player's current position if available, or 0,0,0
            Vector3 playerPos = playerCamera != null ? playerCamera.position : Vector3.zero;
            playerPos.y = 0; // Stay on floor

            float randomX = Random.Range(-5f, 5f);
            float randomZ = Random.Range(15f, 25f);
            
            // Spawn in front of the player's starting orientation (assume Z forward)
            Vector3 spawnPos = playerPos + new Vector3(randomX, 0, randomZ);

            SkeletonEnemy.SkeletonType type = (Random.value > 0.5f) ? SkeletonEnemy.SkeletonType.Red : SkeletonEnemy.SkeletonType.Blue;
            GameObject prefab = (type == SkeletonEnemy.SkeletonType.Red) ? redSkeletonPrefab : blueSkeletonPrefab;

            if (prefab == null)
            {
                Debug.LogError("Skeleton Prefab not assigned in EnemySpawner!");
                return;
            }

            GameObject enemyObj = Instantiate(prefab, spawnPos, Quaternion.identity);
            SkeletonEnemy skeleton = enemyObj.GetComponent<SkeletonEnemy>();
            
            if (skeleton == null) skeleton = enemyObj.AddComponent<SkeletonEnemy>();
            
            skeleton.Initialize(type, 2f, playerCamera, manager);
        }
    }
}
