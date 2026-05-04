using UnityEngine;
using System.Collections.Generic;

public class TileManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private List<GameObject> tilePrefabs;

    [Header("Settings")]
    [Tooltip("Amount of tiles to keep active. 4 is recommended for your sequence.")]
    [SerializeField] private int tilesOnScreen = 4; 
    
    [Tooltip("The exact length of your tile mesh (200 in your case)")]
    [SerializeField] private float tileLength = 200f;

    private List<GameObject> activeTiles = new List<GameObject>();
    private int spawnIndex = 0;

    private void Start()
    {
        // Auto-find player if not assigned
        if (player == null) 
            player = GameObject.FindGameObjectWithTag("Player").transform;

        // Spawn the initial tiles
        for (int i = 0; i < tilesOnScreen; i++)
        {
            SpawnTile(i * tileLength);
        }
    }

    private void Update()
    {
        if (player == null || activeTiles.Count == 0) return;

        // Check if player passed the end of the first tile
        // Logic: If PlayerZ > FirstTileZ + Length, it means we fully walked off it
        if (player.position.z > activeTiles[0].transform.position.z + tileLength)
        {
            MoveFirstTileToEnd();
        }
    }

    private void SpawnTile(float zPos)
    {
        // Spawn the tile based on the current sequence index
        GameObject tile = Instantiate(tilePrefabs[spawnIndex]);
        tile.transform.SetParent(transform);
        tile.transform.position = new Vector3(0, 0, zPos);
        activeTiles.Add(tile);

        // Advance the index, loop back to 0 if it reaches the end of the list
        spawnIndex = (spawnIndex + 1) % tilePrefabs.Count;
    }

    private void MoveFirstTileToEnd()
    {
        // 1. Get the tile that is now behind the player
        GameObject tileToRecycle = activeTiles[0];
        activeTiles.RemoveAt(0);

        // 2. Calculate the new Z position (End of the current last tile)
        float newZ = activeTiles[activeTiles.Count - 1].transform.position.z + tileLength;

        // 3. Move the tile instantly
        tileToRecycle.transform.position = new Vector3(0, 0, newZ);

        // 4. Add it back to the end of the list
        activeTiles.Add(tileToRecycle);
    }
}