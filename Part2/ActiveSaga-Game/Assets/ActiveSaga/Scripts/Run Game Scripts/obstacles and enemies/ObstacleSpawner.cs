using UnityEngine;
using System.Collections.Generic;
using ActiveSaga.RunGame;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private RunGameStatsTracker statsTracker;

    [Header("Track Spawn Area")]
    [SerializeField] private float pathMinX = -1.3f;
    [SerializeField] private float pathMaxX = 1.3f;

    [Header("Spawn Heights")]
    [SerializeField] private float obstacleY = 0f;
    [SerializeField] private float coinY = 1f;
    [SerializeField] private float enemyY = 1f;

    [Header("Enemy Side Spawn")]
    [SerializeField] private float enemySideX = 4f;

    public void Execute(TileInfo tile, List<SpawnRequest> requests)
    {
        if (tile == null)
        {
            return;
        }

        tile.ClearContent();

        Transform parent = tile.contentRoot != null ? tile.contentRoot : tile.transform;

        for (int i = 0; i < requests.Count; i++)
        {
            SpawnableItem item = requests[i].item;

            if (item.prefab == null)
            {
                continue;
            }

            Vector3 spawnPosition = GetSpawnPosition(item.type, requests[i].zPosition);

            GameObject obj = Instantiate(
                item.prefab,
                spawnPosition,
                item.prefab.transform.rotation,
                parent
            );

            SideEnemyRunner enemyRunner = obj.GetComponent<SideEnemyRunner>();

            if (enemyRunner != null)
            {
                enemyRunner.Initialize(player, statsTracker);
            }

            CoinCollectible coin = obj.GetComponent<CoinCollectible>();

            if (coin != null)
            {
                coin.Initialize(statsTracker);
            }

            tile.RegisterSpawnedObject(obj);
        }
    }

    private Vector3 GetSpawnPosition(SpawnType type, float zPosition)
    {
        if (type == SpawnType.Enemy)
        {
            float sideX = Random.value < 0.5f ? -enemySideX : enemySideX;
            return new Vector3(sideX, enemyY, zPosition);
        }

        float randomX = Random.Range(pathMinX, pathMaxX);

        if (type == SpawnType.Collectible)
        {
            return new Vector3(randomX, coinY, zPosition);
        }

        return new Vector3(randomX, obstacleY, zPosition);
    }
}