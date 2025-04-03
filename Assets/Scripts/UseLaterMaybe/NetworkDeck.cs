using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkDeck : NetworkBehaviour
{
    public GameObject cardPrefab;
    private List<Card> deck = new List<Card>();
    private readonly string[] suits = { "hearts", "diamonds", "clubs", "spades" };

    private Card lastDrawnCard;

    public override void OnNetworkSpawn()
    {
        Debug.Log("NetworkDeck Spawned on the server");
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
        if (IsServer) // This should execute only on the server.
        {
            if (deck.Count == 0)
            {
                Debug.LogError("Deck is empty!");
                return;
            }

            lastDrawnCard = deck[0]; // Draw the card
            deck.RemoveAt(0);

            Debug.Log($"Player {playerID} drew: {lastDrawnCard}");

            // Spawn the card for the client
            SpawnCardForClientRpc(playerID, lastDrawnCard.value, lastDrawnCard.suit);
        }
        else
        {
            Debug.LogWarning("DrawCardServerRpc was called by a client instead of the server.");
        }
    }


    [ClientRpc]
    void SpawnCardForClientRpc(ulong playerID, int value, string suit)
    {
        GameObject newCardObj = Instantiate(cardPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        CardDisplay cardDisplay = newCardObj.GetComponent<CardDisplay>();
        if (cardDisplay != null)
        {
           // cardDisplay.SetCard(value, suit);
        }
        Debug.Log($"Player {playerID} received card: {value} of {suit}");
    }

    public Card GetLastCard()
    {
        return lastDrawnCard;
    }
}
