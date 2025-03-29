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
        startHostButton.onClick.AddListener(StartHost);
        startServerButton.onClick.AddListener(StartServer);
        startClientButton.onClick.AddListener(StartClient);

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

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

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Player with Client ID {clientId} connected successfully.");
    }
}
