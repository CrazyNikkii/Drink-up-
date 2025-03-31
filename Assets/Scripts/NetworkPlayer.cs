using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    public NetworkDeck networkDeck;

    private void Start()
    {
        if (networkDeck == null)
        {
            networkDeck = Object.FindFirstObjectByType<NetworkDeck>();
        }
    }

    private void Update()
    {
        if (IsOwner && Input.GetKeyDown(KeyCode.Space)) // Check if spacebar is pressed
        {
            Debug.Log("Spacebar pressed, sending request for card...");
            if (networkDeck != null)
            {
                RequestCardServerRpc(NetworkManager.Singleton.LocalClientId); // Send request for card
            }
            else
            {
                Debug.LogError("NetworkDeck reference missing!");
            }
        }
    }

    [ServerRpc]
    void RequestCardServerRpc(ulong playerID)
    {
        // Server logic to draw a card
        Debug.Log($"Server processing card draw request for player {playerID}");
        networkDeck.DrawCardServerRpc(playerID);
    }
}
