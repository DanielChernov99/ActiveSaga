using UnityEngine;

public enum BiomeType { Forest, City, Space }

public enum SpawnType
{
    Jump,
    Duck,
    Enemy,
    Hazard,
    Collectible
}

[System.Serializable]
public struct SpawnableItem
{
    public string itemName;
    public GameObject prefab;
    public int cost;
    public int weight;
    public SpawnType type;
}

[CreateAssetMenu(fileName = "BiomeData", menuName = "ActiveSaga/Biome Data")]
public class BiomeData : ScriptableObject
{
    public BiomeType biomeType;

    [Header("Spawn Pool")]
    public SpawnableItem[] spawnables;
}