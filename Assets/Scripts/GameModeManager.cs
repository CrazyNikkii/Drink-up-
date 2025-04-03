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
        string currentPlayerUsername = multiplayerManager.GetPlayerName(currentPlayerId); // Get the username of the current player
        string localPlayerUsername = multiplayerManager.GetPlayerName(NetworkManager.Singleton.LocalClientId); // Get the username of the local player

        // Update prompt for the current player
        UpdateGamePromptClientRpc("Red or Black");
        UpdateButtonsForPlayerClientRpc(currentPlayerId);

        if (currentPlayerUsername == localPlayerUsername) // Compare usernames, not IDs
        {
            redButton.onClick.RemoveAllListeners();
            blackButton.onClick.RemoveAllListeners();

            redButton.onClick.AddListener(() => OnColorSelected(true));
            blackButton.onClick.AddListener(() => OnColorSelected(false));
        }

        // Now notify the next player of their turn
        NotifyNextPlayerTurnClientRpc(currentPlayerId);
    }

    private void UpdateButtonsForPlayer(ulong currentPlayerId)
    {
        string currentPlayerName = multiplayerManager.GetPlayerName(currentPlayerId);
        string localPlayerName = multiplayerManager.GetPlayerName(NetworkManager.Singleton.LocalClientId);

        Debug.Log($"Updating buttons for player {currentPlayerName}. LocalPlayer: {localPlayerName}");

        if (currentPlayerName == localPlayerName)
        {
            redButton.gameObject.SetActive(true);
            blackButton.gameObject.SetActive(true);
            Debug.Log($"{localPlayerName} buttons are now visible.");
        }
        else
        {
            redButton.gameObject.SetActive(false);
            blackButton.gameObject.SetActive(false);
            Debug.Log($"{localPlayerName} buttons are hidden for other player.");
        }
    }


    private void OnColorSelected(bool isRed)
    {
        if (!IsServer) return;

        ulong currentPlayerId = playerOrder[currentPlayerIndex];

        Card drawnCard = deck.DrawCard();
        bool cardIsRed = (drawnCard.suit == "hearts" || drawnCard.suit == "diamonds");

        Debug.Log($"Drawn card: {drawnCard}");
        Debug.Log($"Player guessed: {(isRed ? "Red" : "Black")}");
        Debug.Log($"Card is actually: {(cardIsRed ? "Red" : "Black")}");
 
        redButton.gameObject.SetActive(false);
        blackButton.gameObject.SetActive(false);

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
        if (currentPlayerIndex >= playerOrder.Count) currentPlayerIndex = 0;
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
    private void UpdateButtonsForPlayerClientRpc(ulong currentPlayerId)
    {
        Debug.Log($"[ClientRpc] Updating buttons for player {currentPlayerId}. LocalClientId: {NetworkManager.Singleton.LocalClientId}");
        UpdateButtonsForPlayer(currentPlayerId);
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

    [ClientRpc]
    private void NotifyNextPlayerTurnClientRpc(ulong currentPlayerId)
    {
        Debug.Log($"It's now Player {currentPlayerId}'s turn! LocalClientId: {NetworkManager.Singleton.LocalClientId}");
        UpdateButtonsForPlayer(currentPlayerId);
    }
}
