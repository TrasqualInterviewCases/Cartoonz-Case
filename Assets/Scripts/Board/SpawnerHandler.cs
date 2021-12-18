using System.Collections;
using UnityEngine;

public class SpawnerHandler : MonoBehaviour
{
    [SerializeField] Board board;
    [SerializeField] GameObject spawnerPrefab;

    private void Start()
    {
        for (int i = 0; i < board.Width; i++)
        {
            Instantiate(spawnerPrefab, new Vector2(i, board.Height + 1), Quaternion.identity);
        }
    }

}
