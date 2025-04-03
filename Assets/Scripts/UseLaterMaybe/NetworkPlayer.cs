using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    public NetworkDeck networkDeck;

    private void Start()
    {
        Debug.Log($"NetworkPlayer started. IsServer: {IsServer}, IsClient: {IsClient}, IsOwner: {IsOwner}");
        if (networkDeck == null)
        {
            networkDeck = Object.FindFirstObjectByType<NetworkDeck>();
        }
    }

    private void Update()
    {
        // Only handle the Spacebar press if it's the owner (the client) and the server is ready
        if (IsOwner && !IsServer && Input.GetKeyDown(KeyCode.Space)) // Check that this is the client and not server
        {
            Debug.Log("Spacebar pressed, sending request for card...");

            if (networkDeck != null)
            {
                Debug.Log("NetworkDeck is valid, calling DrawCardServerRpc...");
                RequestCardServerRpc(NetworkManager.Singleton.LocalClientId); // Make sure it's the server handling this
            }
            else
            {
                Debug.LogError("NetworkDeck reference missing!");
            }
        }
    }


    public override void OnNetworkSpawn()
    {
        Debug.Log($"NetworkPlayer started. IsServer: {IsServer}, IsClient: {IsClient}, IsOwner: {IsOwner}");
        Debug.Log("NetworkPlayer Spawned on the server");
        DontDestroyOnLoad(gameObject);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestCardServerRpc(ulong playerID)
    {
        Debug.Log($"RequestCardServerRpc CALLED for player {playerID}");

        if (networkDeck == null)
        {
            Debug.LogError("networkDeck is NULL on server! Request ignored.");
            return;
        }

        Debug.Log("NetworkDeck is valid, calling DrawCardServerRpc...");
        Debug.Log($"IsServer: {IsServer}, IsClient: {IsClient}, IsHost: {NetworkManager.Singleton.IsHost}");
        networkDeck.DrawCardServerRpc(playerID);
    }
}
