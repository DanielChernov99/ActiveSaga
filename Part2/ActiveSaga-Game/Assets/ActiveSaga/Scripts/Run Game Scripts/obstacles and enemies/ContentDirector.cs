using UnityEngine;
using System.Collections.Generic;

public enum GameDifficulty { Easy, Medium, Hard }
public enum PacingState { BuildUp, Spike, Recovery }

public class ContentDirector : MonoBehaviour
{
    [Header("State")]
    public GameDifficulty difficulty = GameDifficulty.Easy;

    private PacingState pacing = PacingState.BuildUp;
    private int tileCounter = 0;

    [Header("Data")]
    public List<BiomeData> biomeList;
    private Dictionary<BiomeType, BiomeData> biomeMap;

    [Header("Executor")]
    public ObstacleSpawner spawner;

    private List<SpawnableItem> tempPool = new List<SpawnableItem>();

    // 🔥 anti spam
    private SpawnType lastType = SpawnType.Collectible;

    private void Awake()
    {
        biomeMap = new Dictionary<BiomeType, BiomeData>();

        for (int i = 0; i < biomeList.Count; i++)
        {
            biomeMap[biomeList[i].biomeType] = biomeList[i];
        }
    }

    private void OnEnable() => TileManager.OnTileSpawned += OnTile;
    private void OnDisable() => TileManager.OnTileSpawned -= OnTile;

    private void OnTile(TileInfo tile)
    {
        if (!biomeMap.TryGetValue(tile.biomeType, out BiomeData biome))
            return;

        UpdatePacing();

        int budget = GetBudget();
        int maxItems = tile.availableSpawnPoints.Count;

        List<SpawnableItem> plan = new List<SpawnableItem>();

        while (budget > 0 && plan.Count < maxItems)
        {
            tempPool.Clear();
            int totalWeight = 0;

            for (int i = 0; i < biome.spawnables.Length; i++)
            {
                SpawnableItem item = biome.spawnables[i];

                if (item.cost > budget)
                    continue;

                if (!IsAllowed(item))
                    continue;

                // 🔥 anti spam (no same type twice)
                if (item.type == lastType)
                    continue;

                if (item.weight <= 0)
                    continue;

                tempPool.Add(item);
                totalWeight += item.weight;
            }

            if (tempPool.Count == 0)
                break;

            SpawnableItem chosen = PickWeighted(tempPool, totalWeight);

            plan.Add(chosen);
            budget -= chosen.cost;

            lastType = chosen.type;
        }

        if (plan.Count > 0)
            spawner.Execute(tile, plan);
    }

    private void UpdatePacing()
    {
        tileCounter++;

        if (tileCounter % 5 == 0)
            pacing = PacingState.Spike;
        else if (tileCounter % 5 < 2)
            pacing = PacingState.Recovery;
        else
            pacing = PacingState.BuildUp;
    }

    private int GetBudget()
    {
        int baseBudget =
            difficulty == GameDifficulty.Easy ? 4 :
            difficulty == GameDifficulty.Medium ? 7 : 12;

        if (pacing == PacingState.Spike) return baseBudget + 3;
        if (pacing == PacingState.Recovery) return baseBudget / 2;

        return baseBudget;
    }

    private bool IsAllowed(SpawnableItem item)
    {
        if (pacing == PacingState.Recovery &&
            (item.type == SpawnType.Enemy || item.type == SpawnType.Jump))
        {
            return false;
        }

        return true;
    }

    private SpawnableItem PickWeighted(List<SpawnableItem> items, int totalWeight)
    {
        int r = Random.Range(0, totalWeight);
        int sum = 0;

        for (int i = 0; i < items.Count; i++)
        {
            sum += items[i].weight;
            if (r < sum)
                return items[i];
        }

        return items[0];
    }
}