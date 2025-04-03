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

        Debug.Log("playerorder list count: " + playerOrder.Count);

        if (playerOrder.Count == 0)
        {
            Debug.LogError("No players in the player order list.");
            return;
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
        string currentPlayerUsername = multiplayerManager.GetPlayerName(currentPlayerId);
        string localPlayerUsername = multiplayerManager.GetPlayerName(NetworkManager.Singleton.LocalClientId);

        UpdateGamePromptClientRpc("Red or Black");
        UpdateButtonsForPlayerClientRpc(currentPlayerId);

        if (currentPlayerUsername == localPlayerUsername)
        {
            redButton.onClick.RemoveAllListeners();
            blackButton.onClick.RemoveAllListeners();

            redButton.onClick.AddListener(() => OnColorSelected(true));
            blackButton.onClick.AddListener(() => OnColorSelected(false));
        }

        NotifyNextPlayerTurnClientRpc(currentPlayerId);
    }

    [ClientRpc]
    private void UpdateButtonsForPlayerClientRpc(ulong currentPlayerId)
    {
        Debug.Log($"[ClientRpc] Updating buttons for player {currentPlayerId}. LocalClientId: {NetworkManager.Singleton.LocalClientId}");
        UpdateButtonsForPlayer(currentPlayerId);
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

            redButton.interactable = true;
            blackButton.interactable = true;

            Debug.Log($"{localPlayerName} buttons are now visible and interactable.");
        }
        else
        {
            redButton.gameObject.SetActive(false);
            blackButton.gameObject.SetActive(false);

            redButton.interactable = false;
            blackButton.interactable = false;

            Debug.Log($"{localPlayerName} buttons are hidden and not interactable for other player.");
        }
    }


    private void OnColorSelected(bool isRed)
    {
        Debug.Log($"Button pressed: {(isRed ? "Red" : "Black")}");

        Debug.Log("Currentplayerindex: " + currentPlayerIndex+ " PlayerOrder list count: " + playerOrder.Count);

        if (currentPlayerIndex >= playerOrder.Count)
        {
            Debug.LogError("Current player index is out of bounds.");
            currentPlayerIndex = 0;
        }

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
