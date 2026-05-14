using UnityEngine;
using System.Collections.Generic;

public class TileManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("All Tile Prefabs")]
    [SerializeField] private List<GameObject> tilePrefabs;

    [Header("Settings")]
    [SerializeField] private int tilesOnScreen = 4;
    [SerializeField] private float tileLength = 200f;

    private readonly List<GameObject> activeTiles = new List<GameObject>();
    private readonly List<GameObject> runtimeTilePrefabs = new List<GameObject>();

    private int spawnIndex = 0;
    private BiomeType selectedBiome = BiomeType.Forest;
    private bool biomeWasConfigured = false;

    public static event System.Action<TileInfo> OnTileSpawned;

    public void SetBiome(BiomeType biome)
    {
        selectedBiome = biome;
        biomeWasConfigured = true;

        BuildRuntimeTileList();

        Debug.Log("TileManager biome configured: " + selectedBiome);
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");

            if (p != null)
            {
                player = p.transform;
            }
        }

        if (!biomeWasConfigured)
        {
            Debug.LogWarning("TileManager: Biome was not configured before Start. Falling back to Forest.");
            SetBiome(BiomeType.Forest);
        }

        for (int i = 0; i < tilesOnScreen; i++)
        {
            SpawnTile(i * tileLength);
        }
    }

    private void Update()
    {
        if (player == null || activeTiles.Count == 0)
        {
            return;
        }

        GameObject firstTile = activeTiles[0];

        if (player.position.z > firstTile.transform.position.z + tileLength)
        {
            MoveFirstTileToEnd();
        }
    }

    private void BuildRuntimeTileList()
    {
        runtimeTilePrefabs.Clear();

        for (int i = 0; i < tilePrefabs.Count; i++)
        {
            GameObject prefab = tilePrefabs[i];

            if (prefab == null)
            {
                continue;
            }

            TileInfo info = prefab.GetComponent<TileInfo>();

            if (info == null)
            {
                Debug.LogWarning("Tile prefab is missing TileInfo: " + prefab.name);
                continue;
            }

            if (info.biomeType == selectedBiome)
            {
                runtimeTilePrefabs.Add(prefab);
            }
        }

        spawnIndex = 0;

        if (runtimeTilePrefabs.Count == 0)
        {
            Debug.LogError("TileManager: No tile prefabs found for biome: " + selectedBiome);
        }
        else
        {
            Debug.Log(
                "TileManager found " + runtimeTilePrefabs.Count +
                " tile prefabs for biome: " + selectedBiome
            );
        }
    }

    private void SpawnTile(float zPos)
    {
        if (runtimeTilePrefabs.Count == 0)
        {
            Debug.LogError("TileManager: Cannot spawn tile because runtimeTilePrefabs is empty.");
            return;
        }

        GameObject prefab = runtimeTilePrefabs[spawnIndex];
        GameObject tile = Instantiate(prefab, transform);

        tile.transform.position = new Vector3(0f, 0f, zPos);

        activeTiles.Add(tile);

        NotifyTileSpawned(tile);

        spawnIndex++;

        if (spawnIndex >= runtimeTilePrefabs.Count)
        {
            spawnIndex = 0;
        }
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
        {
            OnTileSpawned?.Invoke(info);
        }
    }
}