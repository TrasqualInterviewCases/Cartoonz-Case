using System.Collections;
using UnityEngine;

public class GamePiece : MonoBehaviour, IInitializable
{
    [SerializeField] GamePieceType type;
    public GamePieceType Type { get { return type; } }

    public int posX;
    public int posY;

    Board board;

    public void Initialize(Vector2 position, Board _board)
    {
        posX = (int)position.x;
        posY = (int)position.y;
        board = _board;

        transform.position = position;

        gameObject.name = "piece: " + posX + ", " + posY;
    }

    public IEnumerator MoveCo(Tile tile, float moveTime)
    {
        var curTime = 0f;

        while (transform.position != tile.transform.position)
        {
            curTime += Time.deltaTime;
            var t = Mathf.Clamp(curTime / moveTime, 0f, 1f);
            transform.position = Vector3.MoveTowards(transform.position, tile.transform.position, t);
            yield return null;
        }
        posX = tile.posX;
        posY = tile.posY;
        board.SetPieceArray(this, posX, posY);
    }
}
