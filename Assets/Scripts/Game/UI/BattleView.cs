using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WordBattle.Core;

namespace WordBattle.Game.UI
{
    /// <summary>
    /// The battle screen: rack tiles, shared timer, word entry with live validation,
    /// lock-in indicators, and the round results overlay. Renders state; holds no rules.
    /// </summary>
    public class BattleView : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;

        [Header("Header")]
        [SerializeField] private TMP_Text roundText;
        [SerializeField] private TMP_Text matchScoreText;
        [SerializeField] private TMP_Text timerText;

        [Header("Rack")]
        [SerializeField] private GameObject[] tiles;
        [SerializeField] private TMP_Text[] tileLetters;
        [SerializeField] private TMP_Text[] tileScores;

        [Header("Input")]
        [SerializeField] private TMP_InputField wordInput;
        [SerializeField] private TMP_Text feedbackText;
        [SerializeField] private Button submitButton;
        [SerializeField] private TMP_Text playerStatusText;
        [SerializeField] private TMP_Text opponentStatusText;

        [Header("Results")]
        [SerializeField] private GameObject resultsPanel;
        [SerializeField] private TMP_Text resultsTitleText;
        [SerializeField] private TMP_Text resultsReasonText;
        [SerializeField] private TMP_Text playerResultText;
        [SerializeField] private TMP_Text opponentResultText;
        [SerializeField] private TMP_Text bestWordText;
        [SerializeField] private Button continueButton;
        [SerializeField] private TMP_Text continueButtonLabel;

        [Header("Colours")]
        [SerializeField] private Color neutralColor = new Color(0.85f, 0.85f, 0.85f);
        [SerializeField] private Color validColor = new Color(0.30f, 0.78f, 0.47f);
        [SerializeField] private Color invalidColor = new Color(0.90f, 0.35f, 0.35f);
        [SerializeField] private Color anagramColor = new Color(0.95f, 0.75f, 0.20f);
        [SerializeField] private Color warningTimeColor = new Color(0.90f, 0.35f, 0.35f);

        private const float WarningSeconds = 5f;

        private bool hasLockedIn;

        private void Awake()
        {
            if (battleManager == null || wordInput == null || submitButton == null || resultsPanel == null ||
                tiles == null || tileLetters == null || tileScores == null)
            {
                Debug.LogError("BattleView: a reference is not assigned in the Inspector.", this);
                enabled = false;
                return;
            }

            wordInput.onValidateInput = (text, index, c) => char.IsLetter(c) ? char.ToUpperInvariant(c) : '\0';
        }

        private void OnEnable()
        {
            battleManager.RoundStarted += OnRoundStarted;
            battleManager.PlayerLockedIn += OnPlayerLockedIn;
            battleManager.RoundEnded += OnRoundEnded;
            wordInput.onValueChanged.AddListener(OnWordChanged);
            wordInput.onSubmit.AddListener(OnWordSubmitted);
            submitButton.onClick.AddListener(OnSubmitClicked);
            continueButton.onClick.AddListener(OnContinueClicked);

            // The first round may have started before this screen was shown.
            Battle battle = battleManager.CurrentBattle;
            if (battle != null)
            {
                OnRoundStarted(battle);
                if (battle.IsResolved)
                {
                    OnRoundEnded(battle.Result);
                }
            }
        }

        private void OnDisable()
        {
            battleManager.RoundStarted -= OnRoundStarted;
            battleManager.PlayerLockedIn -= OnPlayerLockedIn;
            battleManager.RoundEnded -= OnRoundEnded;
            wordInput.onValueChanged.RemoveListener(OnWordChanged);
            wordInput.onSubmit.RemoveListener(OnWordSubmitted);
            submitButton.onClick.RemoveListener(OnSubmitClicked);
            continueButton.onClick.RemoveListener(OnContinueClicked);
        }

        private void Update()
        {
            Battle battle = battleManager.CurrentBattle;
            if (battle == null || timerText == null)
            {
                return;
            }

            float remaining = battle.TimeRemaining;
            timerText.text = Mathf.CeilToInt(remaining).ToString();
            timerText.color = remaining <= WarningSeconds && !battle.IsResolved ? warningTimeColor : neutralColor;
        }

        // --- Gameplay events ---

        private void OnRoundStarted(Battle battle)
        {
            hasLockedIn = false;
            resultsPanel.SetActive(false);

            Match match = battleManager.Match;
            roundText.text = $"Round {match.RoundNumber}  ·  first to {match.RoundsToWin}";
            matchScoreText.text =
                $"You {match.GetWins(battleManager.HumanRole)} – {match.GetWins(battleManager.OpponentRole)} {OpponentName}";

            ShowRack(battle.Rack);

            wordInput.characterLimit = battle.Rack.Size;
            wordInput.interactable = true;
            wordInput.text = string.Empty;
            wordInput.ActivateInputField();

            playerStatusText.text = "Make the best word you can";
            opponentStatusText.text = $"{OpponentName} is thinking…";
            OnWordChanged(string.Empty);
        }

        private void OnPlayerLockedIn(BattleRole role)
        {
            if (role == battleManager.OpponentRole)
            {
                opponentStatusText.text = $"{OpponentName} has locked in";
            }
        }

        private void OnRoundEnded(BattleResult result)
        {
            wordInput.interactable = false;
            submitButton.interactable = false;

            bool humanWon = result.Winner == battleManager.HumanRole;
            resultsTitleText.text = humanWon ? "You win the round!" : $"{OpponentName} wins the round";
            resultsTitleText.color = humanWon ? validColor : invalidColor;
            resultsReasonText.text = DescribeReason(result);

            playerResultText.text = DescribeSubmission("You", result.GetSubmission(battleManager.HumanRole));
            opponentResultText.text = DescribeSubmission(OpponentName, result.GetSubmission(battleManager.OpponentRole));
            bestWordText.text = DescribeBestWord(battleManager.CurrentBattle.Rack);

            Match match = battleManager.Match;
            matchScoreText.text =
                $"You {match.GetWins(battleManager.HumanRole)} – {match.GetWins(battleManager.OpponentRole)} {OpponentName}";
            continueButtonLabel.text = match.IsOver ? "See final result" : "Next round";

            resultsPanel.SetActive(true);
        }

        // --- Input ---

        private void OnWordChanged(string word)
        {
            if (hasLockedIn)
            {
                return;
            }

            WordStatus status = battleManager.Human != null ? battleManager.Human.Validate(word) : WordStatus.Empty;
            int score = WordScorer.CalculateScore(word);

            switch (status)
            {
                case WordStatus.Empty:
                    SetFeedback("Type a word using the tiles above", neutralColor);
                    break;
                case WordStatus.NotInRack:
                    SetFeedback("Uses letters you don't have", invalidColor);
                    break;
                case WordStatus.NotAWord:
                    SetFeedback("Not in the dictionary", invalidColor);
                    break;
                case WordStatus.Valid:
                    SetFeedback($"Valid  ·  {score} pts", validColor);
                    break;
                case WordStatus.Anagram:
                    SetFeedback($"ANAGRAM! Every tile used  ·  {score} pts", anagramColor);
                    break;
            }

            wordInput.textComponent.color = status == WordStatus.Empty ? neutralColor : feedbackText.color;
            submitButton.interactable = WordValidator.IsScoring(status);
        }

        private void OnWordSubmitted(string word)
        {
            TrySubmit(word);
        }

        private void OnSubmitClicked()
        {
            TrySubmit(wordInput.text);
        }

        private void TrySubmit(string word)
        {
            if (hasLockedIn || battleManager.Human == null)
            {
                return;
            }

            SubmitResult result = battleManager.Human.Submit(word);
            if (result == SubmitResult.Accepted)
            {
                hasLockedIn = true;
                wordInput.interactable = false;
                submitButton.interactable = false;
                playerStatusText.text = $"You locked in {WordDictionary.Normalize(word)}";
                SetFeedback("Waiting for the reveal…", neutralColor);
            }
            else if (result == SubmitResult.InvalidWord)
            {
                // Feedback text already explains why; keep the field focused so the player can fix it.
                wordInput.ActivateInputField();
            }
        }

        private void OnContinueClicked()
        {
            battleManager.Continue();
        }

        // --- Helpers ---

        private string OpponentName => battleManager.OpponentProfile.Name;

        private void ShowRack(Rack rack)
        {
            if (rack.Size > tiles.Length)
            {
                Debug.LogError($"BattleView: rack has {rack.Size} tiles but the UI only has {tiles.Length}.", this);
            }

            for (int i = 0; i < tiles.Length; i++)
            {
                bool used = i < rack.Size;
                tiles[i].SetActive(used);
                if (used)
                {
                    char letter = rack.Tiles[i];
                    tileLetters[i].text = letter.ToString();
                    tileScores[i].text = LetterScoreTable.GetLetterScore(letter).ToString();
                }
            }
        }

        private void SetFeedback(string message, Color color)
        {
            feedbackText.text = message;
            feedbackText.color = color;
        }

        private string DescribeSubmission(string who, Submission submission)
        {
            if (submission == null)
            {
                return $"{who}: no word";
            }

            string anagram = submission.Status == WordStatus.Anagram ? "  (anagram!)" : string.Empty;
            return $"{who}: {submission.Word}  —  {submission.Score} pts{anagram}";
        }

        private string DescribeReason(BattleResult result)
        {
            string winner = result.Winner == battleManager.HumanRole ? "You" : OpponentName;

            switch (result.Reason)
            {
                case WinReason.HigherScore:
                    return $"{winner} scored higher";
                case WinReason.FasterSubmission:
                    return $"Tied score — {winner} locked in first";
                default:
                    return result.Attacker == null && result.Defender == null
                        ? "No words locked in — the defender holds"
                        : "Dead heat — the defender holds";
            }
        }

        private static string DescribeBestWord(Rack rack)
        {
            WordDictionary dictionary = DictionaryManager.Instance != null ? DictionaryManager.Instance.Dictionary : null;
            if (dictionary == null)
            {
                return string.Empty;
            }

            List<string> words = WordSolver.FindWords(rack, dictionary);
            return words.Count > 0
                ? $"Best possible: {words[0]}  ({WordScorer.CalculateScore(words[0])} pts)  ·  {words.Count} words in this rack"
                : string.Empty;
        }
    }
}
