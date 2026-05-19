using UnityEngine;

[System.Serializable]
public struct SpawnRequest
{
    public SpawnableItem item;
    public float zPosition;

    public SpawnRequest(SpawnableItem item, float zPosition)
    {
        this.item = item;
        this.zPosition = zPosition;
    }
}