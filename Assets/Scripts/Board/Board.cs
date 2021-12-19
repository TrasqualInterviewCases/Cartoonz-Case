using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] GameObject tilePrefab;
    [SerializeField] int height = 8;
    [SerializeField] int width = 8;

    public int Height { get { return height; } }
    public int Width { get { return width; } }

    List<Tile> spawnedTiles = new List<Tile>();

    private void Start()
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                var spawnedTile = Instantiate(tilePrefab, new Vector2(i, j), Quaternion.identity, transform);
                spawnedTile.name = i + " ," + j;
                spawnedTiles.Add(spawnedTile.GetComponent<Tile>());
            }
        }
    }

    private void SpawnPieces()
    {

    }
}
