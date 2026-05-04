using UnityEngine;
using System.Collections.Generic;

public class TileInfo : MonoBehaviour
{
    [Header("Identity")]
    public BiomeType biomeType;

    [Header("Track Spawn Points (Center - Obstacles)")]
    [Tooltip("Points on the running path (jump / duck obstacles)")]
    public List<Transform> trackSpawnPoints;

    [Header("Side Spawn Points (Enemies)")]
    [Tooltip("Points on the sides (enemies spawn here)")]
    public List<Transform> sideSpawnPoints;

    [Header("Content Root (IMPORTANT)")]
    [Tooltip("All spawned objects will be parented here")]
    public Transform contentRoot;

    // Runtime cache of spawned objects (for cleanup)
    private List<GameObject> spawnedObjects = new List<GameObject>();

    // Called BEFORE new content is spawned
    public void ClearContent()
    {
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            if (spawnedObjects[i] != null)
            {
                Destroy(spawnedObjects[i]);
            }
        }

        spawnedObjects.Clear();
    }

    // Called by Spawner AFTER spawning
    public void RegisterSpawnedObject(GameObject obj)
    {
        if (obj != null)
        {
            spawnedObjects.Add(obj);
        }
    }
}