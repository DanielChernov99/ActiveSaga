using UnityEngine;
using System.Collections.Generic;

public enum GameDifficulty { Easy, Medium, Hard }
public enum PacingState { BuildUp, Spike, Recovery }

public class ContentDirector : MonoBehaviour
{
    [Header("State")]
    public GameDifficulty difficulty = GameDifficulty.Easy;

    [SerializeField] private BiomeType selectedBiome = BiomeType.Forest;

    [Header("Data")]
    public List<BiomeData> biomeList;

    private Dictionary<BiomeType, BiomeData> biomeMap;

    [Header("Executor")]
    public ObstacleSpawner spawner;

    private List<SpawnableItem> tempPool = new List<SpawnableItem>();

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
        TileManager.OnTileSpawned += OnTile;
    }

    private void OnDisable()
    {
        TileManager.OnTileSpawned -= OnTile;
    }

    public void Configure(GameDifficulty newDifficulty, BiomeType newBiome)
    {
        difficulty = newDifficulty;
        selectedBiome = newBiome;

        Debug.Log(
            "ContentDirector configured. Difficulty: " + difficulty +
            ", Biome: " + selectedBiome
        );
    }

    private void OnTile(TileInfo tile)
    {
        if (tile == null)
        {
            return;
        }

        Debug.Log("OnTile CALLED for: " + tile.name);

        if (tile.biomeType != selectedBiome)
        {
            Debug.Log(
                "ContentDirector skipped tile. Tile biome: " +
                tile.biomeType + ", Selected biome: " + selectedBiome
            );

            tile.ClearContent();
            return;
        }

        if (!biomeMap.TryGetValue(tile.biomeType, out BiomeData biome))
        {
            Debug.Log("NO BIOME FOUND for type: " + tile.biomeType);
            return;
        }

        int maxItems = tile.trackSpawnPoints.Count;
        Debug.Log("Spawn points count: " + maxItems);

        if (maxItems == 0)
        {
            Debug.Log("NO SPAWN POINTS - skipping tile");
            return;
        }

        int budget = GetBudget();
        List<SpawnableItem> plan = new List<SpawnableItem>();

        Debug.Log("Spawnables length: " + biome.spawnables.Length);

        while (budget > 0 && plan.Count < maxItems)
        {
            tempPool.Clear();
            int totalWeight = 0;

            for (int i = 0; i < biome.spawnables.Length; i++)
            {
                SpawnableItem item = biome.spawnables[i];

                Debug.Log(
                    "Checking item: " + item.prefab?.name +
                    ", cost=" + item.cost +
                    ", weight=" + item.weight
                );

                if (item.cost > budget)
                {
                    continue;
                }

                if (item.weight <= 0)
                {
                    continue;
                }

                tempPool.Add(item);
                totalWeight += item.weight;
            }

            if (tempPool.Count == 0)
            {
                Debug.Log("TEMP POOL EMPTY - nothing can spawn");
                break;
            }

            SpawnableItem chosen = PickWeighted(tempPool, totalWeight);

            plan.Add(chosen);
            budget -= chosen.cost;
        }

        Debug.Log("PLAN COUNT: " + plan.Count);

        if (plan.Count > 0)
        {
            if (spawner == null)
            {
                Debug.LogError("ContentDirector: Missing ObstacleSpawner reference.");
                return;
            }

            Debug.Log("Calling Spawner.Execute...");
            spawner.Execute(tile, plan);
        }
        else
        {
            tile.ClearContent();
        }
    }

    private int GetBudget()
    {
        return difficulty == GameDifficulty.Easy ? 4 :
               difficulty == GameDifficulty.Medium ? 7 : 12;
    }

    private SpawnableItem PickWeighted(List<SpawnableItem> items, int totalWeight)
    {
        int r = Random.Range(0, totalWeight);
        int sum = 0;

        for (int i = 0; i < items.Count; i++)
        {
            sum += items[i].weight;

            if (r < sum)
            {
                return items[i];
            }
        }

        return items[0];
    }
}