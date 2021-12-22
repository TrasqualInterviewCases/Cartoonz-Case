using UnityEngine;

public class Tile : MonoBehaviour, IInitializable
{
    public int posX;
    public int posY;

    public void Initialize(Vector2 position)
    {
        posX = (int)position.x;
        posY = (int)position.y;

        transform.position = position;

        gameObject.name = posX + ", " + posY;
    }
}
