using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Required for SceneManager

public static class MainDataset
{
    // Static list so it's accessible from anywhere via MainDataset.DocumentGroups
    public static List<List<string>> DocumentGroups { get; private set; } = new List<List<string>>();

    // Two indices to traverse the "Table" (Rows and Columns)
    private static int _globalGroupIndex = 0; // The Row (Day)
    private static int _globalDocIndex = 0;   // The Column (Document within each Day)

    private const string DocumentName = "Sample";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        LoadData();
    }

    // --- NEW METHOD: CheckDay ---
    public static void CheckDay()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        // Logic: Look for "Day" in the scene name and extract the number
        if (sceneName.StartsWith("Day"))
        {
            // Remove "Day" from the string to get just the number (e.g., "Day1" -> "1")
            string dayNumberStr = sceneName.Replace("Day", "");

            if (int.TryParse(dayNumberStr, out int dayNum))
            {
                // Set the Group Index based on Day (Day 1 = Group 0, Day 2 = Group 1, etc.)
                _globalGroupIndex = dayNum - 1;
                _globalDocIndex = 0; // Reset document progress for the new day

                Debug.Log($"<color=yellow>MainDataset:</color> Scene detected as {sceneName}. Setting Group Index to {_globalGroupIndex}");
            }
        }
        else
        {
            Debug.Log($"<color=orange>MainDataset:</color> Current scene '{sceneName}' is not a Day scene. Index remains at {_globalGroupIndex}");
        }
    }

    // Fetches the next document string, traversing columns first
    public static string GetNextDocumentContent()
    {
        if (DocumentGroups.Count == 0) return "No Data Loaded";

        // Check if current group index is valid
        if (_globalGroupIndex < DocumentGroups.Count)
        {
            // --- FIX: Check if we have already exhausted the current row ---
            if (_globalDocIndex >= DocumentGroups[_globalGroupIndex].Count)
            {
                Debug.LogWarning($"MainDataset: Requested document but Group {_globalGroupIndex} is empty!");
                return "End of Day Records";
            }

            // Get the specific document in the current batch
            string content = DocumentGroups[_globalGroupIndex][_globalDocIndex];

            // Increment the Column (Document)
            _globalDocIndex++;

            // --- REMOVED: The auto-increment of _globalGroupIndex ---
            // We no longer move to the next Row automatically. 
            // We let the Scene change (CheckDay) handle that.

            Debug.Log($"Current Doc[{_globalGroupIndex}][{_globalDocIndex - 1}] fetched. Next index: {_globalDocIndex}");

            return content;
        }

        return "End of Records";
    }

    private static void LoadData()
    {
        TextAsset textFile = Resources.Load<TextAsset>(DocumentName);

        if (textFile == null)
        {
            Debug.LogError($"MainDataset: {DocumentName}.txt not found in Resources!");
            return;
        }

        DocumentGroups.Clear();

        string[] allLines = textFile.text.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);
        List<string> currentGroup = new List<string>();

        foreach (string line in allLines)
        {
            string trimmedLine = line.Trim();

            if (trimmedLine == "//")
            {
                if (currentGroup.Count > 0)
                {
                    DocumentGroups.Add(new List<string>(currentGroup));
                    currentGroup.Clear();
                }
            }
            else if (!string.IsNullOrWhiteSpace(trimmedLine))
            {
                currentGroup.Add(trimmedLine);
            }
        }

        if (currentGroup.Count > 0)
        {
            DocumentGroups.Add(currentGroup);
        }

        Debug.Log($"<color=cyan>MainDataset Loaded:</color> {DocumentGroups.Count} groups ready.");

        // --- DEBUG PRINT START ---
        /*
        Debug.Log("<color=cyan><b>MainDataset: Starting Data Dump...</b></color>");

        for (int i = 0; i < DocumentGroups.Count; i++)
        {
            Debug.Log($"<b>Group {i}</b> contains {DocumentGroups[i].Count} documents:");

            for (int j = 0; j < DocumentGroups[i].Count; j++)
            {
                Debug.Log($"   <color=grey>[{j}]:</color> {DocumentGroups[i][j]}");
            }
        }

        Debug.Log("<color=cyan><b>MainDataset: Load Complete.</b></color>");
        */
        // --- DEBUG PRINT END ---
    }

    // Helper to reset if you restart the level
    public static void ResetIndices()
    {
        _globalGroupIndex = 0;
        _globalDocIndex = 0;
    }

    public static bool HasMoreDocumentsInCurrentDay()
    {
        // 1. Safety check: Does this Day even exist in our Big List?
        if (_globalGroupIndex >= DocumentGroups.Count) return false;

        // 2. Grab the specific "Small List" (The Day's Row)
        List<string> currentDayFolder = DocumentGroups[_globalGroupIndex];

        // 3. Check the length of THAT small list
        int totalDocsThisDay = currentDayFolder.Count;

        // 4. Compare our current position to the small list's size
        if (_globalDocIndex < totalDocsThisDay)
        {
            return true; // We still have papers to sign today!
        }

        return false; // Out of papers for this specific Day
    }

    public static int GetGroupIndex()
    {
        return _globalGroupIndex;
    }
}