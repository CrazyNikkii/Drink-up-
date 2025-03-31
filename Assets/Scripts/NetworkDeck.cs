using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkDeck : NetworkBehaviour
{
    public GameObject cardPrefab; // Assign CardPrefab in Inspector
    private List<Card> deck = new List<Card>();
    private readonly string[] suits = { "hearts", "diamonds", "clubs", "spades" };

    private Card lastDrawnCard; // Field to store the last drawn card

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

        // Draw the card from the deck
        lastDrawnCard = deck[0]; // Update the last drawn card
        deck.RemoveAt(0);

        Debug.Log($"Player {playerID} drew: {lastDrawnCard}");

        SpawnCardForClientRpc(playerID, lastDrawnCard.value, lastDrawnCard.suit);
    }

    [ClientRpc]
    void SpawnCardForClientRpc(ulong playerID, int value, string suit)
    {
        // Spawn the card for every client, not just the local client
        GameObject newCardObj = Instantiate(cardPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        CardDisplay cardDisplay = newCardObj.GetComponent<CardDisplay>();
        if (cardDisplay != null)
        {
            cardDisplay.SetCard(value, suit);
        }
        Debug.Log($"Player {playerID} received card: {value} of {suit}");
    }

    // Get the last drawn card
    public Card GetLastCard()
    {
        return lastDrawnCard;
    }
}
