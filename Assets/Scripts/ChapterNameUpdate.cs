using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

public class ChapterNameUpdate : MonoBehaviour
{
    public static ChapterNameUpdate Instance { get; private set; }

    [Header("UI References (TMP)")]
    [SerializeField] private TextMeshProUGUI _chapterText;
    [SerializeField] private TextMeshProUGUI _chapterNameText;
    [SerializeField] private TextMeshProUGUI _descriptorText;
    [SerializeField] private TextMeshProUGUI _endOfChapterText;
    // --- NEW: Reference for the Year text (e.g., (1890-1898)) ---
    [SerializeField] private TextMeshProUGUI _yearText;

    private List<ChapterData> _chapters = new List<ChapterData>();

    private class ChapterData
    {
        public string Chapter;
        public string Name;
        public string Descriptor;
        public string End;
        public string Year;
    }


    private void Start()
    {
        // SceneManager.sceneLoaded does NOT fire for the very first scene when  hit Play.
        // This forces the script to detect the Day and update the UI immediately
        MainDataset.CheckDay();
        UpdateChapterUI(MainDataset.GetGroupIndex());
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        ParseResourceFile();
    }

    private void ParseResourceFile()
    {
        // Ensure this matches your file name in Resources exactly
        TextAsset file = Resources.Load<TextAsset>("ChapterData");

        if (file == null)
        {
            Debug.LogError("ChapterData: Could not find '.txt' file in Resources!");
            return;
        }

        string[] rawSections = file.text.Split(new string[] { "//" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string section in rawSections)
        {
            string[] lines = section.Trim().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // --- Checking for at least 5 lines per section ---
            if (lines.Length >= 5)
            {
                _chapters.Add(new ChapterData
                {
                    Chapter = lines[0].Trim(),
                    Name = lines[1].Trim(),
                    Descriptor = lines[2].Trim().Replace("\\n", "\n"),
                    End = lines[3].Trim(),
                    Year = lines[4].Trim()
                });
            }
        }
    }

    // Pass the index in here from GameManager
    public void UpdateChapterUI(int dayIndex)
    {
        // If for some reason the list is empty, try to parse now
        if (_chapters.Count == 0)
        {
            ParseResourceFile();
        }

        if (dayIndex < 0 || dayIndex >= _chapters.Count)
        {
            Debug.LogError($"ChapterNameUpdate: Index {dayIndex} invalid. Chapter count: {_chapters.Count}");
            return;
        }

        ChapterData data = _chapters[dayIndex];

        // Debug to see if it's actually changing
        Debug.Log($"Updating UI to: {data.Name} with Years: {data.Year}");

        if (_chapterText) _chapterText.text = data.Chapter;
        if (_chapterNameText) _chapterNameText.text = data.Name;
        if (_descriptorText) _descriptorText.text = data.Descriptor;
        if (_endOfChapterText) _endOfChapterText.text = data.End;
        if (_yearText) _yearText.text = data.Year;
    }
}