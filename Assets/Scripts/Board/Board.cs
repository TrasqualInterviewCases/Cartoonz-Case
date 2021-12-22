using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

public class Board : MonoBehaviour
{
    [SerializeField] GameObject tilePrefab;
    [SerializeField] int height = 8;
    [SerializeField] int width = 8;

    public int Height { get { return height; } }
    public int Width { get { return width; } }

    Tile[,] spawnedTiles;
    GamePiece[,] spawnedPieces;

    Tile dragStartTile;
    Tile dragEndTile;

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
                spawnedTile.GetComponent<Tile>().Initialize(new Vector2(i, j), this);
            }
        }
        FillBoard();
    }

    private void FillBoard()
    {
        foreach (var tile in spawnedTiles)
        {
            List<GamePiece> checkListDown = new List<GamePiece>();
            List<GamePiece> checkListLeft = new List<GamePiece>();

            var spawnedPiece = RandomPiece(tile);
            var maxit = 100;
            var it = 0;

            while (CheckForMatchesAtStart(spawnedPiece, checkListDown, checkListLeft))
            {
                it++;
                GamePiecePooler.Instance.RequeuePiece(spawnedPiece.gameObject);
                checkListDown.Clear();
                checkListLeft.Clear();
                if (it < maxit)
                {
                    spawnedPiece = RandomPiece(tile);
                }
                else
                {
                    Debug.Log("looped too many times");
                    break;
                }
            }
        }
    }

    private GamePiece RandomPiece(Tile tile)
    {
        var randPiece = (GamePieceType)Random.Range(0, 4);
        var spawnedPiece = GamePiecePooler.Instance.SpawnFromPool(randPiece, tile.transform.position);
        spawnedPiece.GetComponent<GamePiece>().Initialize(new Vector2(tile.posX, tile.posY), this);
        spawnedPieces[tile.posX, tile.posY] = spawnedPiece.GetComponent<GamePiece>();
        return spawnedPiece;
    }

    private bool CheckForMatchesAtStart(GamePiece spawnedPiece, List<GamePiece> matchingPiecesDown, List<GamePiece> matchingPiecesLeft)
    {
        if (spawnedPiece.posY != 0)
        {
            Debug.Log("checking y" + spawnedPiece.gameObject.name);
            GamePiece nextPieceY;
            for (int i = spawnedPiece.posY; i > 0; i--)
            {
                nextPieceY = spawnedPieces[spawnedPiece.posX, i];
                if (nextPieceY != null)
                {
                    if (nextPieceY.Type == spawnedPiece.Type)
                    {
                        matchingPiecesDown.Add(nextPieceY);
                        Debug.Log("found y match at " + spawnedPiece.gameObject.name);
                    }
                    else
                    {
                        Debug.Log("break at y" + spawnedPiece.gameObject.name);
                        break;
                    }
                }
                else
                {
                    Debug.Log("Next Piece PosY was null");
                    break;
                }
            }
        }

        if (spawnedPiece.posX != 0)
        {
            Debug.Log("checking x" + spawnedPiece.gameObject.name);
            GamePiece nextPieceX;
            for (int j = spawnedPiece.posX; j > 0; j--)
            {
                nextPieceX = spawnedPieces[j, spawnedPiece.posY];
                if (nextPieceX != null)
                {
                    if (nextPieceX.Type == spawnedPiece.Type)
                    {
                        matchingPiecesLeft.Add(nextPieceX);
                        Debug.Log("found x match at " + spawnedPiece.gameObject.name);
                    }
                    else
                    {
                        Debug.Log("break at x" + spawnedPiece.gameObject.name);
                        break;
                    }
                }
                else
                {
                    Debug.Log("Next Piece PosX was null");
                    break;
                }
            }
        }

        return (matchingPiecesDown.Count >= 2 || matchingPiecesLeft.Count >= 2);
    }

    public void SelectStartTile(Tile start)
    {
        dragStartTile = start;
    }

    public void SelectEndTile(Tile end)
    {
        if (dragStartTile != null && end != null)
        {
            dragEndTile = GetCorrectedTile(dragStartTile, end);
        }

    }

    private Tile GetCorrectedTile(Tile start, Tile end)
    {
        Tile correctedTile = null;

        if (start.posX == end.posX)
        {
            correctedTile.posX = start.posX;
            correctedTile.posY = Mathf.Clamp((start.posY - end.posY), -1, 1);
        }

        if (start.posY == end.posY)
        {
            correctedTile.posX = Mathf.Clamp((start.posX - end.posX), -1, 1);
            correctedTile.posY = start.posY;
        }

        return correctedTile;
    }

    public void EndDrag()
    {
        dragStartTile = null;
        dragEndTile = null;
    }

    private IEnumerator MovePieces(Tile start, Tile end)
    {
        var selectedPiece = spawnedPieces[start.posX, start.posY];
        var targetPiece = spawnedPieces[end.posX, end.posY];

        if(selectedPiece != null && targetPiece != null)
        {
            StartCoroutine(selectedPiece.MoveCo(start));
            yield return StartCoroutine(targetPiece.MoveCo(end));
        }

        //if found matches remove matches else move back to starting places
    }

    private void FindMatches()
    {

    }

    private void FindMatchesAtDirection()
    {

    }

}
