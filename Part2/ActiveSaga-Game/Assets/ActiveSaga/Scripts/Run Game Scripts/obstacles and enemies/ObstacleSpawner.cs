using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    public void Execute(TileInfo tile, List<SpawnableItem> plan)
    {
        if (tile == null) return;

        tile.ClearContent();

        List<Transform> trackPoints = new List<Transform>(tile.trackSpawnPoints);
        List<Transform> sidePoints = new List<Transform>(tile.sideSpawnPoints);

        Transform parent = tile.contentRoot != null ? tile.contentRoot : tile.transform;

        for (int i = 0; i < plan.Count; i++)
        {
            SpawnableItem item = plan[i];

            List<Transform> targetPool =
                item.type == SpawnType.Enemy ? sidePoints : trackPoints;

            if (targetPool.Count == 0)
                continue;

            int index = Random.Range(0, targetPool.Count);
            Transform spawnPoint = targetPool[index];

            GameObject obj = Instantiate(
                item.prefab,
                spawnPoint.position,
                spawnPoint.rotation,
                parent
            );

            tile.RegisterSpawnedObject(obj);

            targetPool.RemoveAt(index);
        }
    }
}