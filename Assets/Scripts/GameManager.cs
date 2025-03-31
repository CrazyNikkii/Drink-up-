using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    public Button redButton;
    public Button blackButton;
    public Button higherButton;
    public Button lowerButton;
    public Button insideButton;
    public Button outsideButton;
    public Button heartsButton;
    public Button diamondsButton;
    public Button spadesButton;
    public Button clubsButton;
    public TMP_Text questionText;
    public TMP_Text scoreText;

    private List<ulong> playerIDs = new List<ulong>();
    private int currentPlayerIndex = 0;
    private int[] playerScores;

    private Card lastCard;
    private NetworkDeck networkDeck;
    private int cardCount = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            int totalPlayers = NetworkManager.Singleton.ConnectedClients.Count + 1;
            playerScores = new int[totalPlayers];

            networkDeck = FindAnyObjectByType<NetworkDeck>();

            Debug.Log("Initialized playerScores with size: " + playerScores.Length);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        scoreText = GameObject.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
        questionText = GameObject.Find("QuestionText")?.GetComponent<TextMeshProUGUI>();

        redButton = GameObject.Find("RedButton")?.GetComponent<Button>();
        blackButton = GameObject.Find("BlackButton")?.GetComponent<Button>();
        higherButton = GameObject.Find("HigherButton")?.GetComponent<Button>();
        lowerButton = GameObject.Find("LowerButton")?.GetComponent<Button>();
        insideButton = GameObject.Find("InsideButton")?.GetComponent<Button>();
        outsideButton = GameObject.Find("OutsideButton")?.GetComponent<Button>();
        heartsButton = GameObject.Find("HeartsButton")?.GetComponent<Button>();
        diamondsButton = GameObject.Find("DiamondsButton")?.GetComponent<Button>();
        spadesButton = GameObject.Find("SpadesButton")?.GetComponent<Button>();
        clubsButton = GameObject.Find("ClubsButton")?.GetComponent<Button>();

    }

    public void InitializeGame()
    {
        playerIDs.Clear();

        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            playerIDs.Add(client.Key);
        }

        int totalPlayers = playerIDs.Count;

        playerScores = new int[totalPlayers];

        HideAllButtons();

        StartTurn();
    }

    void HideAllButtons()
    {
        redButton.gameObject.SetActive(false);
        blackButton.gameObject.SetActive(false);
        higherButton.gameObject.SetActive(false);
        lowerButton.gameObject.SetActive(false);
        insideButton.gameObject.SetActive(false);
        outsideButton.gameObject.SetActive(false);
        heartsButton.gameObject.SetActive(false);
        diamondsButton.gameObject.SetActive(false);
        spadesButton.gameObject.SetActive(false);
        clubsButton.gameObject.SetActive(false);
    }

    void ResetButtonListeners()
    {
        redButton.onClick.RemoveAllListeners();
        blackButton.onClick.RemoveAllListeners();
        higherButton.onClick.RemoveAllListeners();
        lowerButton.onClick.RemoveAllListeners();
        insideButton.onClick.RemoveAllListeners();
        outsideButton.onClick.RemoveAllListeners();
        heartsButton.onClick.RemoveAllListeners();
        diamondsButton.onClick.RemoveAllListeners();
        spadesButton.onClick.RemoveAllListeners();
        clubsButton.onClick.RemoveAllListeners();
    }

    void StartTurn()
    {
        cardCount = 0;
        UpdateScore();

        AskRedBlackQuestion();
    }

    void AskRedBlackQuestion()
    {
        HideAllButtons();
        ResetButtonListeners();

        questionText.text = "Is the next card Red or Black";
        redButton.gameObject.SetActive(true);
        blackButton.gameObject.SetActive(true);

        redButton.onClick.AddListener(() => HandleRedBlackChoice("red"));
        blackButton.onClick.AddListener(() => HandleRedBlackChoice("black"));
    }

    void HandleRedBlackChoice(string choice)
    {
        DealCard();
        bool wasCorrect = CheckRedBlackChoice(choice);

        HideAllButtons();
        EndQuestion(wasCorrect);
    }

    bool CheckRedBlackChoice(string choice)
    {
        string cardSuit = lastCard.suit.ToLower();
        bool isRed = cardSuit == "hearts" || cardSuit == "diamonds";
        bool isblack = cardSuit == "spades" || cardSuit == "clubs";

        return (choice == "red" && isRed) || (choice == "black" && isblack);
    }

    void EndQuestion(bool wasCorrect)
    {
        if (!wasCorrect)
        {
            playerScores[currentPlayerIndex]++;
        }

        cardCount++;

        if (cardCount == 1)
        {
            AskHigherLowerQuestion();
        }
        else if (cardCount == 2)
        {
            AskInsideOutsideQuestion();
        }
        else if (cardCount == 3)
        {
            AskSuitQuestion();
        }
        else
        {
            EndTurn();
        }
    }

    void AskHigherLowerQuestion()
    {
        HideAllButtons();
        ResetButtonListeners();

        questionText.text = "Is the next card Higher or Lower?";
        higherButton.gameObject.SetActive(true);
        lowerButton.gameObject.SetActive(true);

        higherButton.onClick.AddListener(() => HandleHigherLowerChoice("higher"));
        lowerButton.onClick.AddListener(() => HandleHigherLowerChoice("lower"));
    }

    void HandleHigherLowerChoice(string choice)
    {
        bool wasCorrect = CheckHigherLowerChoice(choice);
        HideAllButtons();
        EndQuestion(wasCorrect);
    }

    bool CheckHigherLowerChoice(string choice)
    {
        int currentCardValue = lastCard.value;
        int previousCardValue = lastCard.value;

        bool isHigher = currentCardValue > previousCardValue;
        bool isLower = currentCardValue < previousCardValue;

        return (choice == "higher" && isHigher) || (choice == "lower" && isLower);
    }

    void AskInsideOutsideQuestion()
    {
        HideAllButtons();
        ResetButtonListeners();
        questionText.text = "Is the next card Inside or Outside the range of the previous cards?";
        insideButton.gameObject.SetActive(true);
        outsideButton.gameObject.SetActive(true);

        insideButton.onClick.AddListener(() => HandleInsideOutsideChoice("inside"));
        outsideButton.onClick.AddListener(() => HandleInsideOutsideChoice("outside"));
    }

    void HandleInsideOutsideChoice(string choice)
    {
        HideAllButtons();
        bool wasCorrect = CheckInsideOutsideChoice(choice);
        EndQuestion(wasCorrect);
    }

    bool CheckInsideOutsideChoice(string choice)
    {
        int currentCardValue = lastCard.value;
        int firstCardValue = lastCard.value;
        int secondCardValue = lastCard.value;

        int minValue = Mathf.Min(firstCardValue, secondCardValue);
        int maxValue = Mathf.Max(firstCardValue, secondCardValue);

        bool isInside = currentCardValue > minValue && currentCardValue < maxValue;
        bool isOutside = currentCardValue < minValue || currentCardValue > maxValue;

        return (choice == "inside" && isInside) || (choice == "outside" && isOutside);
    }

    void AskSuitQuestion()
    {
        HideAllButtons();
        ResetButtonListeners();
        questionText.text = "What suit will the next card be?";
        heartsButton.gameObject.SetActive(true);
        diamondsButton.gameObject.SetActive(true);
        spadesButton.gameObject.SetActive(true);
        clubsButton.gameObject.SetActive(true);

        heartsButton.onClick.AddListener(() => HandleSuitChoice("hearts"));
        diamondsButton.onClick.AddListener(() => HandleSuitChoice("diamonds"));
        spadesButton.onClick.AddListener(() => HandleSuitChoice("spades"));
        clubsButton.onClick.AddListener(() => HandleSuitChoice("clubs"));
    }

    void HandleSuitChoice(string choice)
    {
        bool wasCorrect = CheckSuitChoice(choice);
        HideAllButtons();
        EndQuestion(wasCorrect);
    }

    bool CheckSuitChoice(string choice)
    {
        string cardSuit = lastCard.suit.ToLower();
        return choice == cardSuit;
    }

    void EndTurn()
    {
        playerScores[currentPlayerIndex]++;
        currentPlayerIndex++;

        if (currentPlayerIndex >= playerIDs.Count)
        {
            EndGame();
        }
        else
        {
            StartTurn();
        }
    }

    void EndGame()
    {
        int winnerIndex = 0;
        for (int i = 1; i < playerScores.Length; i++)
        {
            if (playerScores[i] < playerScores[winnerIndex])
            {
                winnerIndex = i;
            }
        }
    }

    void UpdateScore()
    {
        if (scoreText == null || playerScores.Length == 0)
        {
            Debug.Log("wtf bro");
            return;
        }
        scoreText.text = "Scores:\n";
        for (int i = 0; i < playerScores.Length; i++)
        {
            scoreText.text += $"Player {playerIDs[i]}: {playerScores[i]} drinks\n";
        }
    }

    void DealCard()
    {
        Debug.Log("Draw a card for player;" + playerIDs[currentPlayerIndex]);
        networkDeck.DrawCardServerRpc(playerIDs[currentPlayerIndex]);

        lastCard = networkDeck.GetLastCard();
    }

    public void RestartGame()
    {
        currentPlayerIndex = 0;
        playerScores = new int[NetworkManager.Singleton.ConnectedClients.Count];
        InitializeGame();
    }
}