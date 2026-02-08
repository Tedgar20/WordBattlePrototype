using System.Collections.Generic;
using UnityEngine;

public class DictionaryManager : MonoBehaviour
{
    public static DictionaryManager Instance { get; private set; }

    [SerializeField] private TextAsset dictionaryFile;

    private HashSet<string> words = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadDictionary();
    }

    private void LoadDictionary()
    {
        string[] lines = dictionaryFile.text.Split('\n');

        foreach (string line in lines)
        {
            string word = line.Trim().ToUpper();
            if (!string.IsNullOrEmpty(word))
            {
                words.Add(word);
            }
        }

        Debug.Log($"Dictionary loaded: {words.Count} words");
    }

    public bool IsValidWord(string word)
    {
        return words.Contains(word.ToUpper());
    }
}