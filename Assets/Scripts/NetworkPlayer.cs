using Unity.Netcode;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    public NetworkDeck networkDeck;
    private void Update()
    {
        if (IsOwner && Input.GetKeyDown(KeyCode.Space))
        {
            RequestCardServerRpc(NetworkManager.Singleton.LocalClientId);
        }
        else
        {
            Debug.LogError("NetworkDeck reference missing!");
        }
    }

    [ServerRpc]
    void RequestCardServerRpc(ulong playerID)
    {
        networkDeck.DrawCardServerRpc(playerID);
    }
}
