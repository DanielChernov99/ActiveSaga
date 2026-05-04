using UnityEngine;
using System.Collections.Generic;

public class TileManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private List<GameObject> tilePrefabs;

    [Header("Settings")]
    [SerializeField] private int tilesOnScreen = 4;
    [SerializeField] private float tileLength = 200f;

    private List<GameObject> activeTiles = new List<GameObject>();
    private int spawnIndex = 0;

    public static event System.Action<TileInfo> OnTileSpawned;

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        for (int i = 0; i < tilesOnScreen; i++)
        {
            SpawnTile(i * tileLength);
        }
    }

    private void Update()
    {
        if (player == null || activeTiles.Count == 0) return;

        GameObject firstTile = activeTiles[0];

        if (player.position.z > firstTile.transform.position.z + tileLength)
        {
            MoveFirstTileToEnd();
        }
    }

    private void SpawnTile(float zPos)
    {
        GameObject prefab = tilePrefabs[spawnIndex];
        GameObject tile = Instantiate(prefab, transform);

        tile.transform.position = new Vector3(0f, 0f, zPos);

        activeTiles.Add(tile);

        NotifyTileSpawned(tile);

        spawnIndex++;
        if (spawnIndex >= tilePrefabs.Count)
            spawnIndex = 0;
    }

    private void MoveFirstTileToEnd()
    {
        GameObject tileToRecycle = activeTiles[0];
        activeTiles.RemoveAt(0);

        GameObject lastTile = activeTiles[activeTiles.Count - 1];
        float newZ = lastTile.transform.position.z + tileLength;

        tileToRecycle.transform.position = new Vector3(0f, 0f, newZ);

        activeTiles.Add(tileToRecycle);

        NotifyTileSpawned(tileToRecycle);
    }

    private void NotifyTileSpawned(GameObject tile)
    {
        TileInfo info = tile.GetComponent<TileInfo>();

        if (info != null)
            OnTileSpawned?.Invoke(info);
    }
}