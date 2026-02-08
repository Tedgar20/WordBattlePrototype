using UnityEngine;

public class WordScorer
{
    public static int CalculateScore(string word)
    {
        if (string.IsNullOrWhiteSpace(word)){
            return 0;
        }
        
        int score = 0;

        foreach (char c in word)
        {
            if (char.IsLetter(c))
            {
                score += LetterScoreTable.GetLetterScore(c);
            }
        }

        return score;
    }
}
