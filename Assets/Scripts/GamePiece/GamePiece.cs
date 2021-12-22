using System.Collections;
using UnityEngine;

public class GamePiece : MonoBehaviour, IInitializable
{
    [SerializeField] GamePieceType type;
    public GamePieceType Type { get { return type; } }

    public int posX;
    public int posY;

    public void Initialize(Vector2 position, Board board)
    {
        posX = (int)position.x;
        posY = (int)position.y;

        transform.position = position;

        gameObject.name = "piece: " + posX + ", " + posY;
    }

    public IEnumerator MoveCo(Tile tile)
    {
        while(transform.position != tile.transform.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, tile.transform.position, Time.deltaTime * 5f);
            yield return null;
        }
    }
}
