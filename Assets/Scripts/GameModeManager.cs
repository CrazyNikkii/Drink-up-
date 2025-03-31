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

    private Dictionary<ulong, int> playerScores = new Dictionary<ulong, int>();
    private List<ulong> playerOrder = new List<ulong>();
    private int currentPlayerIndex = 0;

    private void Start()
    {
        redButton.onClick.AddListener(() => OnColorSelected(true));
        blackButton.onClick.AddListener(() => OnColorSelected(false));
        redButton.gameObject.SetActive(false);
        blackButton.gameObject.SetActive(false);
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
        gamePrompt.text = "Red or Black?";
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
        redButton.gameObject.SetActive(false);
        blackButton.gameObject.SetActive(false);

        ulong currentPlayerId = playerOrder[currentPlayerIndex];

        bool correct = Random.Range(0, 2) == (isRed ? 1 : 0);
        if (!correct)
        {
            playerScores[currentPlayerId]++;
        }

        UpdateScores();
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
        scoreText.text = scoreDisplay;
    }
}
