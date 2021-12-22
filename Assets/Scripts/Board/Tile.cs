using UnityEngine;

public class Tile : MonoBehaviour, IInitializable
{
    public int posX;
    public int posY;

	Board board;

    public void Initialize(Vector2 position, Board _board)
    {
        posX = (int)position.x;
        posY = (int)position.y;
		board = _board;
        transform.position = position;

        gameObject.name = posX + ", " + posY;
    }

	void OnMouseDown()
	{
		if (board != null)
		{
			board.SelectStartTile(this);
		}
	}

	void OnMouseEnter()
	{
		if (board != null)
		{
			board.SelectEndTile(this);
		}
	}

	void OnMouseUp()
	{
		if (board != null)
		{
			board.EndDrag();
		}
	}
}
