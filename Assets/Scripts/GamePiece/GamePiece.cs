using UnityEngine;

public class GamePiece : MonoBehaviour, IInitializable
{
    [SerializeField] GamePieceType type;
    public GamePieceType Type { get { return type; } }

    public int posX;
    public int posY;

    public void Initialize(Vector2 position)
    {
        posX = (int)position.x;
        posY = (int)position.y;

        transform.position = position;

        gameObject.name = "piece: " + posX + ", " + posY;
    }
}
