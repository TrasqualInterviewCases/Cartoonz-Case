using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Board : MonoBehaviour
{
    [SerializeField] GameObject tilePrefab;
    [SerializeField] int height = 8;
    [SerializeField] int width = 8;

    public int Height { get { return height; } }
    public int Width { get { return width; } }

    Tile[,] spawnedTiles;
    GamePiece[,] spawnedPieces;

    private void Start()
    {
        spawnedTiles = new Tile[width, height];
        spawnedPieces = new GamePiece[width, height];
        SpawnTiles();

    }

    private void SpawnTiles()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                var spawnedTile = Instantiate(tilePrefab, new Vector2(i, j), Quaternion.identity, transform);
                spawnedTiles[i, j] = spawnedTile.GetComponent<Tile>();
                spawnedTile.GetComponent<Tile>().Initialize(new Vector2(i, j));
            }
        }
        FillBoard();
    }

    private void FillBoard()
    {
        foreach (var tile in spawnedTiles)
        {
            var spawnedPiece = RandomPiece(tile);
            var maxit = 100;
            var it = 0;
            Debug.Log(CheckForMatches(spawnedPiece) + tile.gameObject.name);

            while (CheckForMatches(spawnedPiece))
            {
                it++;
                //GamePiecePooler.Instance.RequeuePiece(spawnedPiece.gameObject);
                Destroy(spawnedPiece.gameObject);
                if (it < maxit)
                {
                    spawnedPiece = RandomPiece(tile);
                }
                else
                {
                    Debug.Log("looped too many times");
                }
            }
        }
    }

    private GamePiece RandomPiece(Tile tile)
    {
        var randPiece = (GamePieceType)Random.Range(0, 4);
        var spawnedPiece = GamePiecePooler.Instance.SpawnFromPool(randPiece, tile.transform.position);
        spawnedPieces[tile.posX, tile.posY] = spawnedPiece.GetComponent<GamePiece>();
        spawnedPiece.GetComponent<GamePiece>().Initialize(tile.transform.position);
        return spawnedPiece;
    }

    private bool CheckForMatches(GamePiece spawnedPiece)
    {
        List<GamePiece> matchingPiecesDown = new List<GamePiece>();
        List<GamePiece> matchingPiecesLeft = new List<GamePiece>();

        if (spawnedPiece.posY != 0)
        {
            for (int i = spawnedPiece.posY; i > 0; i--)
            {
                var nextPiece = spawnedPieces[spawnedPiece.posX, i];
                if (nextPiece != null)
                {
                    if (nextPiece.Type == spawnedPiece.Type)
                    {
                        matchingPiecesDown.Add(nextPiece);
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    Debug.Log("Next Piece PosY was null");
                }
            }
        }

        if (spawnedPiece.posX != 0)
        {
            for (int j = spawnedPiece.posX; j > 0; j--)
            {
                var nextPiece = spawnedPieces[j, spawnedPiece.posY];
                if (nextPiece != null)
                {
                    if (nextPiece.Type == spawnedPiece.Type)
                    {
                        matchingPiecesLeft.Add(nextPiece);
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    Debug.Log("Next Piece PosX was null");
                }
            }
        }

        if (matchingPiecesDown.Count > 2 || matchingPiecesLeft.Count > 2)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
