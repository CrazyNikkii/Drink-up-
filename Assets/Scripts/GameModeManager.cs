using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameModeManager : NetworkBehaviour
{
    public MultiplayerManager multiplayerManager;
    public TMP_Text gamePrompt;
    public Button redButton, blackButton;
    public TMP_Text scoreText;
    public GameObject cardPrefab;

    private Dictionary<ulong, int> playerScores = new Dictionary<ulong, int>();
    private List<ulong> playerOrder = new List<ulong>();
    private int currentPlayerIndex = 0;
    private Deck deck;

    private void Start()
    {
        redButton.onClick.AddListener(() => OnColorSelected(true));
        blackButton.onClick.AddListener(() => OnColorSelected(false));
        redButton.gameObject.SetActive(false);
        blackButton.gameObject.SetActive(false);

        deck = new Deck();
    }

    public void StartGame()
    {
        if (!IsServer) return;

        playerOrder.Clear();
        playerScores.Clear();

        foreach (var player in multiplayerManager.GetPlayers())
        {
            playerOrder.Add(player.Key);
            playerScores[player.Key] = 0;
        }

        currentPlayerIndex = 0;
        StartTurn();
    }

    private void StartTurn()
    {
        if (currentPlayerIndex >= playerOrder.Count)
        {
            currentPlayerIndex = 0;
        }

        ulong currentPlayerId = playerOrder[currentPlayerIndex];
        UpdateGamePromptClientRpc("Red or Black");
        UpdateButtonsForPlayer(currentPlayerId);
    }

    private void UpdateButtonsForPlayer(ulong currentPlayerId)
    {
        if (NetworkManager.Singleton.LocalClientId == currentPlayerId)
        {
            redButton.gameObject.SetActive(true);
            blackButton.gameObject.SetActive(true);
        }
        else
        {
            redButton.gameObject.SetActive(false);
            blackButton.gameObject.SetActive(false);
        }
    }

    private void OnColorSelected(bool isRed)
    {
        if (!IsServer) return;

        Card drawnCard = deck.DrawCard();
        bool cardIsRed = (drawnCard.suit == "hearts" || drawnCard.suit == "diamonds");

        Debug.Log($"Drawn card: {drawnCard}");
        Debug.Log($"Player guessed: {(isRed ? "Red" : "Black")}");
        Debug.Log($"Card is actually: {(cardIsRed ? "Red" : "Black")}");

        redButton.gameObject.SetActive(false);
        blackButton.gameObject.SetActive(false);

        ulong currentPlayerId = playerOrder[currentPlayerIndex];

        if (cardIsRed != isRed)
        {
            playerScores[currentPlayerId]++;
        }

        UpdateScores();

        Vector3 spawnPosition = CardSpawnManager.Instance.GetSpawnPosition(currentPlayerId);

        GameObject newCard = Instantiate(cardPrefab, spawnPosition, Quaternion.identity);
        NetworkObject cardNetworkObject = newCard.GetComponent<NetworkObject>();
        cardNetworkObject.Spawn();
        ShowCardClientRpc(cardNetworkObject.NetworkObjectId, drawnCard.value, drawnCard.suit, currentPlayerId);

        currentPlayerIndex++;
        StartTurn();
    }

    private void UpdateScores()
    {
        string scoreDisplay = "Scores:\n";
        foreach (var player in playerScores)
        {
            scoreDisplay += $"{multiplayerManager.GetPlayerName(player.Key)}: {player.Value}\n";
        }
        UpdateScoresClientRpc(scoreDisplay);
    }

    [ClientRpc]
    private void UpdateGamePromptClientRpc(string promptText)
    {
        gamePrompt.gameObject.SetActive(true);
        gamePrompt.text = promptText;
    }

    [ClientRpc]
    private void UpdateScoresClientRpc(string scoreDisplay)
    {
        scoreText.text = scoreDisplay;
    }

    [ClientRpc]
    private void ShowCardClientRpc(ulong cardObjectId, int value, string suit, ulong playerId)
    {
        Debug.Log($"[Client] Showing card: {value} of {suit} for Player {playerId}");

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(cardObjectId, out NetworkObject netObj))
        {
            CardDisplay display = netObj.GetComponent<CardDisplay>();
            if (display != null)
            {
                Debug.Log($"[Client] Found card object! Updating texture...");
                display.SetCard(value, suit, playerId);
            }
            else
            {
                Debug.LogError($"[Client] Card object exists but missing CardDisplay component!");
            }
        }
        else
        {
            Debug.LogError($"[Client] Card object with ID {cardObjectId} not found!");
        }
    }
}
