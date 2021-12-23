using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections;

public class Board : MonoBehaviour
{
    [SerializeField] GameObject tilePrefab;
    [SerializeField] int height = 8;
    [SerializeField] int width = 8;
    [SerializeField] float pieceMoveTime = 0.2f;

    public int Height { get { return height; } }
    public int Width { get { return width; } }

    Tile[,] spawnedTiles;
    GamePiece[,] spawnedPieces;

    Tile dragStartTile;
    Tile dragEndTile;

    GamePiecePooler pooler;

    bool canDrag = true;

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
        SetPieceArray(spawnedPiece.GetComponent<GamePiece>(), tile.posX, tile.posY);
        return spawnedPiece;
    }

    public void SetPieceArray(GamePiece piece, int i, int j)
    {
        spawnedPieces[i, j] = piece;
    }

    private bool CheckForMatchesAtStart(GamePiece spawnedPiece, List<GamePiece> matchingPiecesDown, List<GamePiece> matchingPiecesLeft)
    {
        if (spawnedPiece.posY != 0)
        {
            //Debug.Log("checking y" + spawnedPiece.gameObject.name);
            GamePiece nextPieceY;
            for (int i = spawnedPiece.posY; i > 0; i--)
            {
                nextPieceY = spawnedPieces[spawnedPiece.posX, i];
                if (nextPieceY != null)
                {
                    if (nextPieceY.Type == spawnedPiece.Type)
                    {
                        matchingPiecesDown.Add(nextPieceY);
                        //Debug.Log("found y match at " + spawnedPiece.gameObject.name);
                    }
                    else
                    {
                        //Debug.Log("break at y" + spawnedPiece.gameObject.name);
                        break;
                    }
                }
                else
                {
                    //Debug.Log("Next Piece PosY was null");
                    break;
                }
            }
        }

        if (spawnedPiece.posX != 0)
        {
            //Debug.Log("checking x" + spawnedPiece.gameObject.name);
            GamePiece nextPieceX;
            for (int j = spawnedPiece.posX; j > 0; j--)
            {
                nextPieceX = spawnedPieces[j, spawnedPiece.posY];
                if (nextPieceX != null)
                {
                    if (nextPieceX.Type == spawnedPiece.Type)
                    {
                        matchingPiecesLeft.Add(nextPieceX);
                        //Debug.Log("found x match at " + spawnedPiece.gameObject.name);
                    }
                    else
                    {
                        //Debug.Log("break at x" + spawnedPiece.gameObject.name);
                        break;
                    }
                }
                else
                {
                    //Debug.Log("Next Piece PosX was null");
                    break;
                }
            }
        }

        return (matchingPiecesDown.Count >= 2 || matchingPiecesLeft.Count >= 2);
    }

    public void SelectStartTile(Tile start)
    {
        if (!canDrag) return;
        if (dragStartTile == null)
            dragStartTile = start;
    }

    public void SelectEndTile(Tile end)
    {
        if (!canDrag) return;
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
            pos.y = start.posY + Mathf.Clamp((end.posY - start.posY), -1, 1);
        }

        if (start.posY == end.posY)
        {
            pos.x = start.posX + Mathf.Clamp((end.posX - start.posX), -1, 1);
            pos.y = start.posY;
        }

        return (start.posX != end.posX && start.posY != end.posY) ? null : spawnedTiles[(int)pos.x, (int)pos.y];
    }

    public void EndDrag()
    {
        if (!canDrag) return;
        if (dragStartTile != null && dragEndTile != null)
            StartCoroutine(MovePieces(dragStartTile, dragEndTile));
        dragStartTile = null;
        dragEndTile = null;
    }

    private IEnumerator MovePieces(Tile start, Tile end)
    {

        var selectedPiece = spawnedPieces[start.posX, start.posY];
        var targetPiece = spawnedPieces[end.posX, end.posY];

        if (selectedPiece != null && targetPiece != null)
        {
            if (selectedPiece.Type == targetPiece.Type) yield break;

            canDrag = false;
            StartCoroutine(selectedPiece.MoveCo(end, pieceMoveTime));
            StartCoroutine(targetPiece.MoveCo(start, pieceMoveTime));


            yield return new WaitForSeconds(pieceMoveTime + 0.1f);

            var selectedMatches = FindAllMatches(selectedPiece);
            //Debug.Log(selectedMatches.Count);
            var targetMatches = FindAllMatches(targetPiece);
            //Debug.Log(targetMatches.Count);

            if (selectedMatches.Count == 0 && targetMatches.Count == 0)
            {
                StartCoroutine(selectedPiece.MoveCo(start, pieceMoveTime));
                StartCoroutine(targetPiece.MoveCo(end, pieceMoveTime));
                yield return new WaitForSeconds(pieceMoveTime + 0.1f);
                canDrag = true;
            }
            else
            {
                var allmatches = selectedMatches.Union(targetMatches).ToList();
                StartCoroutine(DespawnAndReAdjust(allmatches));
            }
        }
    }

    private List<GamePiece> FindAllMatches(GamePiece startPiece, int minCount = 3)
    {
        var allMatches = new List<GamePiece>();
        var upMatches = FindMatchesAtDirection(startPiece, new Vector2(0, 1), 2);
        var downMatches = FindMatchesAtDirection(startPiece, new Vector2(0, -1), 2);
        var leftMatches = FindMatchesAtDirection(startPiece, new Vector2(-1, 0), 2);
        var rightMatches = FindMatchesAtDirection(startPiece, new Vector2(1, 0), 2);

        if (upMatches == null)
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
        horMatches = horMatches.Count >= minCount ? horMatches : null;
        if (horMatches == null)
        {
            horMatches = new List<GamePiece>();
        }

        var verMatches = leftMatches.Union(rightMatches).ToList();
        verMatches = verMatches.Count >= minCount ? verMatches : null;
        if (verMatches == null)
        {
            verMatches = new List<GamePiece>();
        }

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
                else
                {
                    break;
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

        return (x >= 0 && x <= width - 1 && y >= 0 && y <= height - 1);
    }

    private List<Tile> DespawnPieces(List<GamePiece> matchedPieces)
    {
        if (matchedPieces == null) return null;
        List<Tile> emptyTiles = new List<Tile>();
        foreach (var piece in matchedPieces)
        {
            spawnedPieces[piece.posX, piece.posY] = null;
            emptyTiles.Add(spawnedTiles[piece.posX, piece.posY]);
            pooler.RequeuePiece(piece.gameObject);
        }
        return emptyTiles;
    }

    private List<GamePiece> ReAdjustColumns(List<GamePiece> matches)
    {
        List<GamePiece> allMovedPieces = new List<GamePiece>();
        foreach (var column in ColumnsToReAdjust(matches))
        {
            allMovedPieces = allMovedPieces.Union(ReAdjustColumn(column)).ToList();
        }
        return allMovedPieces;
    }

    private List<GamePiece> ReAdjustColumn(int column)
    {
        var movedPieces = new List<GamePiece>();

        for (int i = 0; i < height - 1; i++)
        {
            if (spawnedPieces[column, i] == null)
            {
                for (int j = i + 1; j < height; j++)
                {
                    if (spawnedPieces[column, j] != null)
                    {
                        var movingPiece = spawnedPieces[column, j];
                        StartCoroutine(movingPiece.MoveCo(spawnedTiles[column, i], 5f));
                        movingPiece.posY = i;
                        spawnedPieces[column, i] = spawnedPieces[column, j];
                        spawnedPieces[column, j] = null;
                        movedPieces.Add(movingPiece);
                        break;
                    }
                }
            }
        }
        canDrag = true;
        return movedPieces;
    }

    private List<int> ColumnsToReAdjust(List<GamePiece> matches)
    {
        List<int> columns = new List<int>();
        foreach (var piece in matches)
        {
            if (!columns.Contains(piece.posX))
            {
                columns.Add(piece.posX);
            }
        }
        return columns;
    }

    private IEnumerator DespawnAndReAdjust(List<GamePiece> matches)
    {
        List<GamePiece> allNewMatches = new List<GamePiece>();
        List<GamePiece> movedPieces;

        DespawnPieces(matches);
        yield return new WaitForSeconds(0.25f);
        movedPieces = ReAdjustColumns(matches);
        while (!AdjustmentCompleted(movedPieces))
        {
            yield return null;
        }
        foreach (var piece in movedPieces)
        {
            var newMatches = FindAllMatches(piece);
            allNewMatches = allNewMatches.Union(newMatches).ToList();
            //Debug.Log(allNewMatches.Count);
        }
        if (allNewMatches.Count > 0)
        {
            StartCoroutine(DespawnAndReAdjust(allNewMatches));
        }
        else
        {
            StartCoroutine(RespawnPieces(FindEmptyTiles()));
            canDrag = true;
        }
    }

    private bool AdjustmentCompleted(List<GamePiece> movingPieces)
    {
        foreach (var piece in movingPieces)
        {
            if (piece != null)
            {
                if (piece.isMoving) return false;
            }
        }
        return true;
    }

    private IEnumerator RespawnPieces(List<Tile> emptyTiles)
    {
        List<GamePiece> newSpawns = new List<GamePiece>();
        foreach (var emptyTile in emptyTiles)
        {
            var spawnedNewPiece = RandomPiece(emptyTile);
            spawnedNewPiece.transform.position = new Vector2(emptyTile.posX, height + 1);
            StartCoroutine(spawnedNewPiece.MoveCo(emptyTile, 5f));
            spawnedNewPiece.posY = emptyTile.posY;
            spawnedPieces[spawnedNewPiece.posX, spawnedNewPiece.posY] = spawnedNewPiece;
            newSpawns.Add(spawnedNewPiece.GetComponent<GamePiece>());
        }
        while (!AdjustmentCompleted(newSpawns))
        {
            yield return null;
        }
        List<GamePiece> newSpawnMatches = new List<GamePiece>();
        foreach (var piece in newSpawns)
        {
            newSpawnMatches = newSpawnMatches.Union(FindAllMatches(piece)).ToList();
        }
        //Debug.Log(newSpawnMatches.Count);
        if (newSpawnMatches.Count > 0)
        {
            StartCoroutine(DespawnAndReAdjust(newSpawnMatches));
        }
    }

    List<Tile> FindEmptyTiles()
    {
        List<Tile> emptyTiles = new List<Tile>();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (spawnedPieces[i, j] == null)
                {
                    emptyTiles.Add(spawnedTiles[i, j]);
                }
            }

        }

        return emptyTiles;
    }
}
