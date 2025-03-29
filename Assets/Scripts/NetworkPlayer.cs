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
        // Only let the owner of the player object do this
        if (IsOwner && Input.GetKeyDown(KeyCode.Space))
        {
            if (networkDeck != null)
            {
                RequestCardServerRpc(NetworkManager.Singleton.LocalClientId);
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
        networkDeck.DrawCardServerRpc(playerID);
    }
}
