using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkDeck : NetworkBehaviour
{
    private List<Card> deck = new List<Card>();
    private readonly string[] suits = { "Hearts", "Diamonds", "Clubs", "Spades" };

    public override void OnNetworkSpawn()
    {
        if (IsServer) InitializeDeck();
    }

    void InitializeDeck()
    {
        deck.Clear();

        foreach (string suit in suits)
        {
            for (int value = 1; value <= 13; value++)
            {
                deck.Add(new Card(value, suit));
            }
        }

        ShuffleDeck();
    }

    void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int randomIndex = Random.Range(i, deck.Count);
            (deck[i], deck[randomIndex]) = (deck[randomIndex], deck[i]);
        }
    }


    [ServerRpc]
    public void DrawCardServerRpc(ulong playerID)
    {
        if (deck.Count == 0) return;

        Card drawnCard = deck[0];
        deck.RemoveAt(0);

        Debug.Log($"Player {playerID} drew: {drawnCard}");

        GiveCardToPlayerClientRpc(playerID, drawnCard.value, drawnCard.suit);
    }

    [ClientRpc]
    void GiveCardToPlayerClientRpc(ulong playerID, int value, string suit)
    {
        if (NetworkManager.Singleton.LocalClientId == playerID)
        {
            Card receivedCard = new Card(value, suit);
            Debug.Log($"Player {playerID} received card: {receivedCard}");
        }
    }
}
