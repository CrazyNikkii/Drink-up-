using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Collections;
using System.Collections.Generic;

public class MultiplayerManager : NetworkBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField ipInput;
    public Button confirmButton;
    public Button hostButton;
    public Button joinButton;
    public Button startGameButton;
    public TMP_Text playerListText;

    private Dictionary<ulong, string> players = new Dictionary<ulong, string>();
    private string playerName = "Player";

    private void Start()
    {
        hostButton.gameObject.SetActive(false);
        joinButton.gameObject.SetActive(false);
        ipInput.gameObject.SetActive(false);
        startGameButton.gameObject.SetActive(false);

        confirmButton.onClick.AddListener(SetUsername);
        hostButton.onClick.AddListener(StartHost);
        joinButton.onClick.AddListener(StartClient);
        startGameButton.onClick.AddListener(StartGame);

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void SetUsername()
    {
        if (!string.IsNullOrEmpty(usernameInput.text))
        {
            playerName = usernameInput.text;
        }

        hostButton.gameObject.SetActive(true);
        joinButton.gameObject.SetActive(true);
        confirmButton.gameObject.SetActive(false);
        usernameInput.gameObject.SetActive(false);
        ipInput.gameObject.SetActive(true);
    }

    private void StartHost()
    {
        var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
        transport.SetConnectionData("0.0.0.0", 7777);

        if (NetworkManager.Singleton.StartHost())
        {
            RegisterPlayer(NetworkManager.Singleton.LocalClientId, playerName);
            hostButton.gameObject.SetActive(false);
            joinButton.gameObject.SetActive(false);
            ipInput.gameObject.SetActive(false);
            startGameButton.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("Failed to start host.");
        }
    }

    private void StartClient()
    {
        string ipAddress = ipInput.text;
        if (!string.IsNullOrEmpty(ipAddress))
        {
            NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>().SetConnectionData(ipAddress, 7777);

            NetworkManager.Singleton.StartClient();
            hostButton.gameObject.SetActive(false);
            joinButton.gameObject.SetActive(false);
            ipInput.gameObject.SetActive(false);
            startGameButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Please enter a valid IP address.");
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected.");
        if (IsServer)
        {
            RequestUsernameClientRpc(clientId);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (players.ContainsKey(clientId))
        {
            players.Remove(clientId);
            UpdatePlayerList();
        }
    }

    private void RegisterPlayer(ulong clientId, string name)
    {
        if (!players.ContainsKey(clientId))
        {
            players[clientId] = name;
            UpdatePlayerList();
        }
    }

    private void UpdatePlayerList()
    {
        playerListText.text = "Players:\n";
        foreach (KeyValuePair<ulong, string> player in players)
        {
            playerListText.text += player.Value + " (ID: " + player.Key + ")\n";
        }
    }

    private void StartGame()
    {
        if (IsServer)
        {
            StartGameClientRpc();
            startGameButton.gameObject.SetActive(false);
        }
    }

    public Dictionary<ulong, string> GetPlayers()
    {
        return players;
    }

    public string GetPlayerName(ulong clientId)
    {
        return players.ContainsKey(clientId) ? players[clientId] : "Unknown";
    }

    [ClientRpc]
    private void RequestUsernameClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            SendUsernameServerRpc(playerName);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendUsernameServerRpc(string name, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        RegisterPlayer(clientId, name);

        ulong[] clientIds = new ulong[players.Count];
        FixedString32Bytes[] names = new FixedString32Bytes[players.Count];

        int index = 0;
        foreach (KeyValuePair<ulong, string> player in players)
        {
            clientIds[index] = player.Key;
            names[index] = new FixedString32Bytes(player.Value);
            index++;
        }

        SyncPlayerListClientRpc(clientIds, names);
    }

    [ClientRpc]
    private void SyncPlayerListClientRpc(ulong[] clientIds, FixedString32Bytes[] names)
    {
        players.Clear();
        for (int i = 0; i < clientIds.Length; i++)
        {
            players[clientIds[i]] = names[i].ToString();
        }
        UpdatePlayerList();
    }

    [ClientRpc]
    private void StartGameClientRpc()
    {
        GameModeManager gameModeManager = FindAnyObjectByType<GameModeManager>();
        if (gameModeManager != null)
        {
            gameModeManager.StartGame();
        }
    }

}
