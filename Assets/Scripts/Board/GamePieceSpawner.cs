using System.Collections.Generic;
using UnityEngine;

public class GamePieceSpawner : MonoBehaviour
{
    [SerializeField] List<GameObject> spawnablePieces = new List<GameObject>();

    private void SpawnAt(Vector2 pos)
    {
        Instantiate(RandomPiece(), pos, Quaternion.identity);
    }

    private GameObject RandomPiece()
    {
        var rand = Random.Range(0, spawnablePieces.Count);
        return spawnablePieces[rand];
    }
}
