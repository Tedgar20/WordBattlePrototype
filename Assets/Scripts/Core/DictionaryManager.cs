using System;
using System.Collections.Generic;
using UnityEngine;

public class DictionaryManager : MonoBehaviour
{
    public static DictionaryManager Instance { get; private set; }

    [SerializeField] private TextAsset dictionaryFile;
    [SerializeField] private int maxWordLength = 8;

    private readonly HashSet<string> words = new HashSet<string>(StringComparer.Ordinal);

    public int WordCount => words.Count;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadDictionary();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void LoadDictionary()
    {
        if (dictionaryFile == null)
        {
            Debug.LogError("DictionaryManager: no dictionary file assigned in the Inspector.", this);
            return;
        }

        string[] lines = dictionaryFile.text.Split('\n');

        foreach (string line in lines)
        {
            string word = line.Trim().ToUpperInvariant();
            if (!string.IsNullOrEmpty(word) && word.Length <= maxWordLength)
            {
                words.Add(word);
            }
        }

        Debug.Log($"Dictionary loaded: {words.Count} words (max length {maxWordLength})");
    }

    public bool IsValidWord(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return false;
        }

        return words.Contains(word.Trim().ToUpperInvariant());
    }
}
