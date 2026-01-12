using UnityEngine;
using System.Collections.Generic;

public class TileManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the player to track their position")]
    public Transform player;

    [Tooltip("List of the Tile prefabs to cycle through")]
    public List<GameObject> tilePrefabs;

    [Header("Settings")]
    [Tooltip("Number of tiles to spawn initially")]
    public int initialTiles = 3; 

    [Tooltip("Length of one tile in units")]
    public float tileLength = 200f;    

    [Tooltip("Distance behind the player before a tile is recycled")]
    public float safeZone = 30f;

    [Header("Visual Fixes")]
    [Tooltip("Manual offset to adjust position if models are not centered or too low")]
    public Vector3 visualOffset = Vector3.zero;

    // Internal list to track active tiles
    private List<GameObject> activeTiles = new List<GameObject>();

    void Start()
    {
        // 1. Player Setup
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }

        // Reset player position as requested
        if (player != null)
        {
            player.position = new Vector3(0, 1, 0);
        }

        // 2. Spawn Initial Tiles
        for (int i = 0; i < initialTiles; i++)
        {
            SpawnTile(i);
        }
    }

    void Update()
    {
        if (player == null || activeTiles.Count == 0) return;

        // Check if player passed the first tile based on safeZone
        float firstTileEndZ = activeTiles[0].transform.position.z + tileLength;

        if (player.position.z > firstTileEndZ + safeZone)
        {
            RecycleTile();
        }
    }

    void SpawnTile(int positionIndex)
    {
        // Create new tile
        GameObject tile = Instantiate(tilePrefabs[Random.Range(0, tilePrefabs.Count)]);
        tile.transform.SetParent(transform);
        
        // Calculate Position: Index * Length + Offset
        float zPos = positionIndex * tileLength;
        tile.transform.position = new Vector3(0, 0, zPos) + visualOffset;

        activeTiles.Add(tile);
    }

    void RecycleTile()
    {
        // 1. Get the oldest tile (first in list)
        GameObject tile = activeTiles[0];
        activeTiles.RemoveAt(0);

        // 2. Calculate new Z position based on the LAST tile in the list
        // We take the Z of the last tile + one length unit
        float newZ = activeTiles[activeTiles.Count - 1].transform.position.z + tileLength;
        
        // 3. Move the tile instantly (Teleport)
        tile.transform.position = new Vector3(0, 0, newZ) + visualOffset;

        // 4. Add back to the end of the list
        activeTiles.Add(tile);
    }
}