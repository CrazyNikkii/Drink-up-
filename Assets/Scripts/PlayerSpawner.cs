using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    // Reference to the player prefab (PlayerObject)
    public GameObject playerPrefab;

    private void Start()
    {
        // Register the event for when a client joins
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    // This is called when a client successfully connects to the host
    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
        {
            // Log that we're trying to spawn a player
            Debug.Log($"Spawning player for client {clientId}");

            GameObject playerObject = Instantiate(playerPrefab);
            NetworkObject networkObject = playerObject.GetComponent<NetworkObject>();
            networkObject.SpawnAsPlayerObject(clientId);

            Debug.Log($"Player object spawned for client {clientId}");
        }
    }
}
