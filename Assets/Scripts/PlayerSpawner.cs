using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
        {
            Debug.Log($"Spawning player for client {clientId}");

            GameObject playerObject = Instantiate(playerPrefab);
            NetworkObject networkObject = playerObject.GetComponent<NetworkObject>();
            networkObject.SpawnAsPlayerObject(clientId);

            Debug.Log($"Player object spawned for client {clientId}");
        }
    }
}
