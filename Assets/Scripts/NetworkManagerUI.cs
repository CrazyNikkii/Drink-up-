using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode.Transports.UTP;

public class NetworkManagerUI : MonoBehaviour
{
    public Button startHostButton;
    public Button startServerButton;
    public Button startClientButton;

    void Start()
    {
        // Attach button events
        startHostButton.onClick.AddListener(StartHost);
        startServerButton.onClick.AddListener(StartServer);
        startClientButton.onClick.AddListener(StartClient);

        // Register the OnClientConnectedCallback event to log when a player connects
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    // Start Host (Server + Client on the same instance)
    void StartHost()
    {
        Debug.Log("Start Host Button Pressed");
        if (NetworkManager.Singleton.StartHost())
        {
            Debug.Log("Host Started");
        }
        else
        {
            Debug.Log("Failed to Start Host");
        }
    }

    // Start Server only
    void StartServer()
    {
        Debug.Log("Start Server Button Pressed");
        if (NetworkManager.Singleton.StartServer())
        {
            Debug.Log("Server Started");
        }
        else
        {
            Debug.Log("Failed to Start Server");
        }
    }

    public void StartClient()
    {
        Debug.Log("StartClient called");
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager.Singleton is null!");
            return;
        }
        NetworkManager.Singleton.StartClient();
    }

    // Event handler for when a client successfully connects to the server
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Player with Client ID {clientId} connected successfully.");
    }
}
