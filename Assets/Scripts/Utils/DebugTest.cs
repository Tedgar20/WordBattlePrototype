using UnityEngine;

public class DebugTest : MonoBehaviour
{
    private void Start()
    {
        string testWord = "QUIZ";

        bool valid = DictionaryManager.Instance.IsValidWord(testWord);
        int score = WordScorer.CalculateScore(testWord);

        Debug.Log($"Word: {testWord}, Valid: {valid}, Score: {score}");
    }
}