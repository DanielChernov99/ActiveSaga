using UnityEngine;
using System.Collections.Generic;

public enum GameDifficulty { Easy, Medium, Hard }

public class ContentDirector : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private GameDifficulty difficulty = GameDifficulty.Easy;
    [SerializeField] private BiomeType selectedBiome = BiomeType.Forest;

    [Header("Data")]
    [SerializeField] private List<BiomeData> biomeList;

    [Header("Executor")]
    [SerializeField] private ObstacleSpawner spawner;

    [Header("Tile Settings")]
    [SerializeField] private float tileLength = 400f;
    [SerializeField] private float tileFrontPadding = 25f;
    [SerializeField] private float tileBackPadding = 25f;

    [Header("Obstacle Distance Settings")]
    [SerializeField] private Vector2 easyObstacleDistance = new Vector2(130f, 170f);
    [SerializeField] private Vector2 mediumObstacleDistance = new Vector2(80f, 120f);
    [SerializeField] private Vector2 hardObstacleDistance = new Vector2(40f, 60f);

    [Header("Enemy Distance Settings")]
    [SerializeField] private Vector2 easyEnemyDistance = new Vector2(220f, 300f);
    [SerializeField] private Vector2 mediumEnemyDistance = new Vector2(140f, 210f);
    [SerializeField] private Vector2 hardEnemyDistance = new Vector2(80f, 140f);

    [Header("Coin Distance Settings")]
    [SerializeField] private Vector2 easyCoinDistance = new Vector2(60f, 100f);
    [SerializeField] private Vector2 mediumCoinDistance = new Vector2(50f, 90f);
    [SerializeField] private Vector2 hardCoinDistance = new Vector2(40f, 80f);

    private Dictionary<BiomeType, BiomeData> biomeMap;

    private float nextObstacleDistance;
    private float nextEnemyDistance;
    private float nextCoinDistance;

    private void Awake()
    {
        biomeMap = new Dictionary<BiomeType, BiomeData>();

        for (int i = 0; i < biomeList.Count; i++)
        {
            if (biomeList[i] == null)
            {
                continue;
            }

            biomeMap[biomeList[i].biomeType] = biomeList[i];
        }
    }

    private void OnEnable()
    {
        TileManager.OnTileSpawned += OnTileSpawned;
    }

    private void OnDisable()
    {
        TileManager.OnTileSpawned -= OnTileSpawned;
    }

    public void Configure(GameDifficulty newDifficulty, BiomeType newBiome)
    {
        difficulty = newDifficulty;
        selectedBiome = newBiome;

        nextObstacleDistance = GetRandomNextDistance(GetObstacleRange());
        nextEnemyDistance = GetRandomNextDistance(GetEnemyRange());
        nextCoinDistance = GetRandomNextDistance(GetCoinRange());

        Debug.Log("ContentDirector configured: " + difficulty + ", " + selectedBiome);
    }

    private void OnTileSpawned(TileInfo tile)
    {
        if (tile == null)
        {
            return;
        }

        tile.ClearContent();

        if (tile.biomeType != selectedBiome)
        {
            return;
        }

        if (!biomeMap.TryGetValue(tile.biomeType, out BiomeData biome))
        {
            Debug.LogWarning("No BiomeData found for: " + tile.biomeType);
            return;
        }

        float tileStartZ = tile.transform.position.z + tileFrontPadding;
        float tileEndZ = tile.transform.position.z + tileLength - tileBackPadding;

        List<SpawnRequest> requests = new List<SpawnRequest>();

        AddRequestsForType(
            biome,
            SpawnType.Jump,
            ref nextObstacleDistance,
            GetObstacleRange(),
            tileStartZ,
            tileEndZ,
            requests
        );

        AddRequestsForType(
            biome,
            SpawnType.Enemy,
            ref nextEnemyDistance,
            GetEnemyRange(),
            tileStartZ,
            tileEndZ,
            requests
        );

        AddRequestsForType(
            biome,
            SpawnType.Collectible,
            ref nextCoinDistance,
            GetCoinRange(),
            tileStartZ,
            tileEndZ,
            requests
        );

        if (requests.Count > 0)
        {
            spawner.Execute(tile, requests);
        }
    }

    private void AddRequestsForType(
        BiomeData biome,
        SpawnType type,
        ref float nextDistance,
        Vector2 distanceRange,
        float tileStartZ,
        float tileEndZ,
        List<SpawnRequest> requests
    )
    {
        while (nextDistance >= tileStartZ && nextDistance < tileEndZ)
        {
            SpawnableItem? item = PickRandomByType(biome, type);

            if (item.HasValue)
            {
                requests.Add(new SpawnRequest(item.Value, nextDistance));
            }

            nextDistance += Random.Range(distanceRange.x, distanceRange.y);
        }
    }

    private float GetRandomNextDistance(Vector2 range)
    {
        return Random.Range(range.x, range.y);
    }

    private SpawnableItem? PickRandomByType(BiomeData biome, SpawnType type)
    {
        List<SpawnableItem> options = new List<SpawnableItem>();
        int totalWeight = 0;

        for (int i = 0; i < biome.spawnables.Length; i++)
        {
            SpawnableItem item = biome.spawnables[i];

            if (item.prefab == null)
            {
                continue;
            }

            if (item.type != type)
            {
                continue;
            }

            if (item.weight <= 0)
            {
                continue;
            }

            options.Add(item);
            totalWeight += item.weight;
        }

        if (options.Count == 0)
        {
            return null;
        }

        int randomValue = Random.Range(0, totalWeight);
        int current = 0;

        for (int i = 0; i < options.Count; i++)
        {
            current += options[i].weight;

            if (randomValue < current)
            {
                return options[i];
            }
        }

        return options[0];
    }

    private Vector2 GetObstacleRange()
    {
        if (difficulty == GameDifficulty.Easy)
        {
            return easyObstacleDistance;
        }

        if (difficulty == GameDifficulty.Medium)
        {
            return mediumObstacleDistance;
        }

        return hardObstacleDistance;
    }

    private Vector2 GetEnemyRange()
    {
        if (difficulty == GameDifficulty.Easy)
        {
            return easyEnemyDistance;
        }

        if (difficulty == GameDifficulty.Medium)
        {
            return mediumEnemyDistance;
        }

        return hardEnemyDistance;
    }

    private Vector2 GetCoinRange()
    {
        if (difficulty == GameDifficulty.Easy)
        {
            return easyCoinDistance;
        }

        if (difficulty == GameDifficulty.Medium)
        {
            return mediumCoinDistance;
        }

        return hardCoinDistance;
    }
}