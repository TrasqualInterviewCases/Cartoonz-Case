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

    GamePiecePooler pooler;

    private void Start()
    {
        pooler = GamePiecePooler.Instance;

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
                pooler.RequeuePiece(spawnedPiece.gameObject);
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
        var spawnedPiece = pooler.SpawnFromPool(randPiece, tile.transform.position);
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
        
        var pos = new Vector2();
        if (start.posX == end.posX)
        {
            pos.x = start.posX;
            pos.y = Mathf.Clamp((start.posY - end.posY), -1, 1);
        }

        if (start.posY == end.posY)
        {
            pos.x = Mathf.Clamp((start.posX - end.posX), -1, 1);
            pos.y = start.posY;
        }
        return spawnedTiles[(int)pos.x, (int)pos.y];
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

        if (selectedPiece != null && targetPiece != null)
        {
            StartCoroutine(selectedPiece.MoveCo(start));
            yield return StartCoroutine(targetPiece.MoveCo(end));
        }

        var selectedMatches = FindAllMatches(selectedPiece);
        var targetMatches = FindAllMatches(targetPiece);

        if(selectedMatches.Count == 0 && targetMatches.Count == 0)
        {
            StartCoroutine(selectedPiece.MoveCo(end));
            yield return StartCoroutine(targetPiece.MoveCo(start));
        }
        else
        {
            DespawnPieces(selectedMatches);
            DespawnPieces(targetMatches);
        }
    }

    private List<GamePiece> FindAllMatches(GamePiece startPiece, int minCount = 3)
    {
        var allMatches = new List<GamePiece>();
        var upMatches = FindMatchesAtDirection(startPiece, new Vector2(0, 1), 3);
        var downMatches = FindMatchesAtDirection(startPiece, new Vector2(0, -1), 3);
        var leftMatches = FindMatchesAtDirection(startPiece, new Vector2(0, -1), 3);
        var rightMatches = FindMatchesAtDirection(startPiece, new Vector2(0, 1), 3);

        if(upMatches == null)
        {
            upMatches = new List<GamePiece>();
        }

        if (downMatches == null)
        {
            downMatches = new List<GamePiece>();
        }

        if (leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }

        if (rightMatches == null)
        {
            rightMatches = new List<GamePiece>();
        }

        var horMatches = upMatches.Union(downMatches).ToList();
        horMatches = horMatches.Count > minCount ? horMatches : null;

        var verMatches = leftMatches.Union(rightMatches).ToList();
        verMatches = verMatches.Count > minCount ? verMatches : null;

        allMatches = horMatches.Union(verMatches).ToList();

        return allMatches;
    }

    private List<GamePiece> FindMatchesAtDirection(GamePiece startPiece, Vector2 direction, int minCount = 3)
    {
        var matches = new List<GamePiece>();

        if (startPiece != null)
        {
            matches.Add(startPiece);
        }
        else { return null; }

        int max = (width > height) ? width : height;

        for (int i = 1; i < max - 1; i++)
        {
            var nextPos = new Vector2(startPiece.posX, startPiece.posY) + direction * i;

            if (ValidatePosition(nextPos))
            {
                var nextPiece = spawnedPieces[(int)nextPos.x, (int)nextPos.y];
                if (nextPiece != null)
                {
                    if (nextPiece.Type == startPiece.Type && !matches.Contains(nextPiece))
                    {
                        matches.Add(nextPiece);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        if (matches.Count >= minCount)
        {
            return matches;
        }

        return null;
    }

    private bool ValidatePosition(Vector2 pos)
    {
        var x = pos.x;
        var y = pos.y;

        return (x >= 0 && x <= width && y >= 0 && y <= height);
    }

    private void DespawnPieces(List<GamePiece> matchedPieces)
    {
        if (matchedPieces == null) return;
        foreach (var piece in matchedPieces)
        {
            pooler.RequeuePiece(piece.gameObject);
        }
    }
}
