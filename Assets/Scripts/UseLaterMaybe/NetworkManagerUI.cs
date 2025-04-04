using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NetworkManagerUI : MonoBehaviour
{
    public Button startHostButton;
    public Button startServerButton;
    public Button startClientButton;

    public Button startGameButton;

    void Start()
    {
        startHostButton.onClick.AddListener(StartHost);
        startServerButton.onClick.AddListener(StartServer);
        startClientButton.onClick.AddListener(StartClient);
        startGameButton.onClick.AddListener(StartGame);

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    void StartHost()
    {
        Debug.Log("Start Host Button Pressed");
        if (NetworkManager.Singleton.StartHost())
        {
            Debug.Log("Host Started");
            SceneManager.LoadScene("GameScene");
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
        SceneManager.LoadScene("GameScene");
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Player with Client ID {clientId} connected successfully.");

        if (NetworkManager.Singleton.IsHost)
        {
            startGameButton.gameObject.SetActive(true);
        }

    }

    private void StartGame()
    {
        GameManager.Instance.InitializeGame();
    }
}
