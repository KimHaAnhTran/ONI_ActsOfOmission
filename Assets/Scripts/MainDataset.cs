using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MainDataset
{
    // Static list so it's accessible from anywhere via MainDataset.DocumentGroups
    public static List<List<string>> DocumentGroups { get; private set; } = new List<List<string>>();

    // Two indices to traverse the "Table" (Rows and Columns)
    private static int _globalGroupIndex = 0; // The Row (Batch)
    private static int _globalDocIndex = 0;   // The Column (Document within Batch)

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        LoadData();
    }

    // Fetches the next document string, traversing columns first, then moving to the next row
    public static string GetNextDocumentContent()
    {
        if (DocumentGroups.Count == 0) return "No Data Loaded";


        Debug.Log("Bypassed first if-else GetNextDocumentContent");
        // Check if current group index is valid
        if (_globalGroupIndex < DocumentGroups.Count)
        {
            // Get the specific document in the current batch
            string content = DocumentGroups[_globalGroupIndex][_globalDocIndex];

            // Increment the Column (Document)
            _globalDocIndex++;

            // If we've reached the end of the current Row (Batch), move to the next Row
            if (_globalDocIndex >= DocumentGroups[_globalGroupIndex].Count)
            {
                _globalDocIndex = 0;
                _globalGroupIndex++;
            }

            Debug.Log($"Current Doc[{_globalGroupIndex}][{_globalDocIndex}]");

            return content;
        }

        return "End of Records";
    }

    private static void LoadData()
    {
        TextAsset textFile = Resources.Load<TextAsset>("Documents");

        if (textFile == null)
        {
            Debug.LogError("MainDataset: Documents.txt not found in Resources!");
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
}