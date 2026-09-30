using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TextMeshPro

public class GameStateManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text playerText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_InputField wordInputField;
    [SerializeField] private Button submitButton;

    [Header("Game Settings")]
    [SerializeField] private int totalPlayers = 2;
    [SerializeField] private float turnTime = 30f;

    private int currentPlayerIndex = 0; 
    private float turnTimer;

    private int[] playerScores;

    private void Awake()
    {
        playerScores = new int[totalPlayers];

        if (playerText == null || timerText == null || scoreText == null ||
            wordInputField == null || submitButton == null)
        {
            Debug.LogError("GameStateManager: one or more UI references are not assigned in the Inspector.", this);
            enabled = false;
        }
    }

    private void Start()
    {
        submitButton.onClick.AddListener(OnSubmitButtonPressed);

        StartTurn();
        UpdateScoreUI();
    }

    private void OnDestroy()
    {
        if (submitButton != null)
        {
            submitButton.onClick.RemoveListener(OnSubmitButtonPressed);
        }
    }

    private void Update()
    {
        if (wordInputField.interactable)
        {
            turnTimer -= Time.deltaTime;
            UpdateTimerUI();

            if (turnTimer <= 0f)
            {
                Debug.Log("Time's up!");
                EndTurn();
            }
        }
    }

    // --- Turn Management ---
    private void StartTurn()
    {
        turnTimer = turnTime;

        playerText.text = $"Player {currentPlayerIndex + 1}'s Turn";
        wordInputField.interactable = true;
        submitButton.interactable = true;

        wordInputField.text = "";
        wordInputField.ActivateInputField(); // Focus
    }

    private void EndTurn()
    {
        wordInputField.interactable = false;
        submitButton.interactable = false;

        currentPlayerIndex = (currentPlayerIndex + 1) % totalPlayers;

        StartTurn();
        UpdateScoreUI();
    }

    // --- Word Submission ---
    private void OnSubmitButtonPressed()
    {
        string word = wordInputField.text.Trim().ToUpperInvariant();

        if (string.IsNullOrEmpty(word))
            return;

        if (DictionaryManager.Instance == null)
        {
            Debug.LogError("GameStateManager: no DictionaryManager in the scene.");
            return;
        }

        if (!DictionaryManager.Instance.IsValidWord(word))
        {
            Debug.Log("Invalid word!");
            return;
        }

        int score = WordScorer.CalculateScore(word);
        playerScores[currentPlayerIndex] += score;

        Debug.Log($"Player {currentPlayerIndex + 1} scored {score} points!");

        EndTurn();
    }

    // --- UI Updates ---
    private void UpdateTimerUI()
    {
        timerText.text = $"Time: {Mathf.CeilToInt(turnTimer)}s";
    }

    private void UpdateScoreUI()
    {
        string scoreString = "";
        for (int i = 0; i < totalPlayers; i++)
        {
            scoreString += $"Player {i + 1}: {playerScores[i]}  ";
        }
        scoreText.text = scoreString;
    }
}