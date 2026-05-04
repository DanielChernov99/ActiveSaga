using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    public void Execute(TileInfo tile, List<SpawnableItem> plan)
    {
        if (tile == null) return;

        //  CRITICAL: clear old content
        tile.ClearContent();

        List<Transform> points = new List<Transform>(tile.availableSpawnPoints);

        Transform parent = tile.contentRoot != null ? tile.contentRoot : tile.transform;

        for (int i = 0; i < plan.Count; i++)
        {
            if (points.Count == 0)
                break;

            int index = Random.Range(0, points.Count);
            Transform p = points[index];

            GameObject obj = Instantiate(plan[i].prefab, p.position, p.rotation, parent);

            // 🔥 register for cleanup
            tile.RegisterSpawnedObject(obj);

            points.RemoveAt(index);
        }
    }
}