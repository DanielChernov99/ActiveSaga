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

    // 🔥 Event that ContentDirector listens to
    public static event System.Action<TileInfo> OnTileSpawned;

    private void Start()
    {
        // Find player automatically if not assigned
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;
            else
            {
                Debug.LogError("Player not found (Tag = Player missing)");
                return;
            }
        }

        // Safety check
        if (tilePrefabs == null || tilePrefabs.Count == 0)
        {
            Debug.LogError("TileManager: No tile prefabs assigned!");
            return;
        }

        // Spawn initial tiles
        for (int i = 0; i < tilesOnScreen; i++)
        {
            SpawnTile(i * tileLength);
        }
    }

    private void Update()
    {
        if (player == null || activeTiles.Count == 0)
            return;

        GameObject firstTile = activeTiles[0];

        if (firstTile == null)
            return;

        // If player passed the tile completely → recycle
        if (player.position.z > firstTile.transform.position.z + tileLength)
        {
            MoveFirstTileToEnd();
        }
    }

    private void SpawnTile(float zPos)
    {
        GameObject prefab = tilePrefabs[spawnIndex];
        if (prefab == null) return;

        GameObject tile = Instantiate(prefab, transform);

        tile.transform.position = new Vector3(0f, 0f, zPos);

        activeTiles.Add(tile);

        // 🔥 Notify Director
        NotifyTileSpawned(tile);

        // Loop index safely
        spawnIndex++;
        if (spawnIndex >= tilePrefabs.Count)
            spawnIndex = 0;
    }

    private void MoveFirstTileToEnd()
    {
        GameObject tileToRecycle = activeTiles[0];
        activeTiles.RemoveAt(0);

        if (tileToRecycle == null)
            return;

        GameObject lastTile = activeTiles[activeTiles.Count - 1];
        float newZ = lastTile.transform.position.z + tileLength;

        // Move tile forward
        tileToRecycle.transform.position = new Vector3(0f, 0f, newZ);

        activeTiles.Add(tileToRecycle);

        // 🔥 CRITICAL: Notify again (new content must be generated)
        NotifyTileSpawned(tileToRecycle);
    }

    // ✅ Centralized event call (cleaner + safer)
    private void NotifyTileSpawned(GameObject tile)
    {
        TileInfo info = tile.GetComponent<TileInfo>();

        if (info != null)
        {
            OnTileSpawned?.Invoke(info);
        }
        else
        {
            Debug.LogWarning("Tile has no TileInfo component!");
        }
    }

    public List<GameObject> GetActiveTiles()
    {
        return activeTiles;
    }
}