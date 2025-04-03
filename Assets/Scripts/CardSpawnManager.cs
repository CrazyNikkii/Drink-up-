using UnityEngine;
using System.Collections.Generic;

public class CardSpawnManager : MonoBehaviour
{
    public static CardSpawnManager Instance {  get; private set; }

    private Dictionary<ulong, Transform> playerSpawnPoints = new Dictionary<ulong, Transform>();

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform spawnPoint = transform.GetChild(i);
            playerSpawnPoints[(ulong)i] = spawnPoint;
        }
    }

    public Vector3 GetSpawnPosition(ulong playerId)
    {
        if (playerSpawnPoints.ContainsKey(playerId))
        {
            return playerSpawnPoints[playerId].position;
        }

        Debug.LogWarning($"No Spawn point found for {playerId}, using default (0,0,0)");
        return Vector3.zero;
    }
}
