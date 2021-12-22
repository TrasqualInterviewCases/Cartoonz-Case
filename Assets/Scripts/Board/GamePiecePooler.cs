using System.Collections.Generic;
using UnityEngine;

public class GamePiecePooler : MonoBehaviour
{
    [SerializeField] List<GamePiecePool> pools = new List<GamePiecePool>();
    Dictionary<GamePieceType, Queue<GameObject>> queueDictionary = new Dictionary<GamePieceType, Queue<GameObject>>();
    Dictionary<GamePieceType, GamePiecePool> poolDictionary = new Dictionary<GamePieceType, GamePiecePool>();

    public static GamePiecePooler Instance;
    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        foreach (var pool in pools)
        {
            Queue<GameObject> piecePool = new Queue<GameObject>();
            var poolParent = new GameObject(pool.type.ToString() + " pool");
            poolParent.transform.SetParent(transform);
            for (int i = 0; i < pool.poolSize; i++)
            {
                var spawnedPiece = Instantiate(pool.prefab, poolParent.transform);
                spawnedPiece.SetActive(false);
                piecePool.Enqueue(spawnedPiece);
            }

            poolDictionary.Add(pool.type, pool);
            queueDictionary.Add(pool.type, piecePool);
        }
    }

    public GamePiece SpawnFromPool(GamePieceType type, Vector3 pos)
    {
        GameObject piece;
        if (queueDictionary[type].Count <= 0)
        {
            piece = Instantiate(poolDictionary[type].prefab);
            piece.SetActive(false);
            queueDictionary[type].Enqueue(piece);
        }

        piece = queueDictionary[type].Dequeue();
        

        piece.transform.position = pos;
        piece.SetActive(true);

        if (queueDictionary[type].Count > poolDictionary[type].poolSize)
        {
            for (int i = 0; i < queueDictionary[type].Count - poolDictionary[type].poolSize; i++)
            {
                var extraPiece = queueDictionary[type].Dequeue();
                Destroy(extraPiece);
            }
        }

        return piece.GetComponent<GamePiece>();
    }

    public void RequeuePiece(GameObject piece)
    {
        piece.transform.position = transform.position;
        piece.SetActive(false);
        queueDictionary[piece.GetComponent<GamePiece>().Type].Enqueue(piece);
    }
}
