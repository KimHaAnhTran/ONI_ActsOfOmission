using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MainDataset
{
    // Static list so it's accessible from anywhere via MainDataset.DocumentGroups
    public static List<List<string>> DocumentGroups { get; private set; } = new List<List<string>>();

    // This attribute tells Unity to run this method automatically as soon as the game boots up
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        LoadData();
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
        // --- DEBUG PRINT END ---
    }
}