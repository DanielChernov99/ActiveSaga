using UnityEngine;
using System.Collections.Generic;

public class TileInfo : MonoBehaviour
{
    [Header("Identity")]
    public BiomeType biomeType;

    [Header("Track Spawn Points (Obstacles)")]
    public List<Transform> trackSpawnPoints;

    [Header("Side Spawn Points (Enemies)")]
    public List<Transform> sideSpawnPoints;

    [Header("Content Root")]
    public Transform contentRoot;

    private List<GameObject> spawnedObjects = new List<GameObject>();

    public void ClearContent()
    {
        for (int i = 0; i < spawnedObjects.Count; i++)
        {
            if (spawnedObjects[i] != null)
                Destroy(spawnedObjects[i]);
        }

        spawnedObjects.Clear();
    }

    public void RegisterSpawnedObject(GameObject obj)
    {
        if (obj != null)
            spawnedObjects.Add(obj);
    }
}