using UnityEngine;
using WordBattle.Core;

namespace WordBattle.Game
{
    /// <summary>
    /// Loads the word list TextAsset once and shares the resulting WordDictionary across scenes.
    /// </summary>
    public class DictionaryManager : MonoBehaviour
    {
        public static DictionaryManager Instance { get; private set; }

        [SerializeField] private TextAsset dictionaryFile;
        [SerializeField] private int maxWordLength = 8;

        public WordDictionary Dictionary { get; private set; }

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

            Dictionary = WordDictionary.FromText(dictionaryFile.text, maxWordLength);

            Debug.Log($"Dictionary loaded: {Dictionary.Count} words (max length {maxWordLength})");
        }

        public bool IsValidWord(string word)
        {
            return Dictionary != null && Dictionary.Contains(word);
        }
    }
}
