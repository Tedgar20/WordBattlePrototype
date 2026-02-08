using UnityEngine;
using UnityEngine.UI;
using TMPro; // For TextMeshPro

public class GameStateManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text playerText;      
    public TMP_Text timerText;       
    public TMP_Text scoreText;       
    public TMP_InputField wordInputField; 
    public Button submitButton;      

    [Header("Game Settings")]
    public int totalPlayers = 2;     
    public float turnTime = 30f;     

    private int currentPlayerIndex = 0; 
    private float turnTimer;

    private int[] playerScores;

    private void Awake()
    {
        playerScores = new int[totalPlayers];
    }

    private void Start()
    {
        submitButton.onClick.AddListener(OnSubmitButtonPressed);

        StartTurn();
        UpdateScoreUI();
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
        string word = wordInputField.text.Trim().ToUpper();

        if (string.IsNullOrEmpty(word))
            return;

        // Validate word (requires your DictionaryManager)
        if (!DictionaryManager.Instance.IsValidWord(word))
        {
            Debug.Log("Invalid word!");
            return;
        }

        // Calculate word score using LetterScoreTable
        int score = CalculateWordScore(word);
        playerScores[currentPlayerIndex] += score;

        Debug.Log($"Player {currentPlayerIndex + 1} scored {score} points!");

        EndTurn();
    }

    private int CalculateWordScore(string word)
    {
        int score = 0;
        foreach (char c in word)
        {
            score += LetterScoreTable.GetLetterScore(c);
        }
        return score;
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