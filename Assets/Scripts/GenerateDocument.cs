using UnityEngine;
using System.Collections.Generic;
using System;

public class GenerateDocument : MonoBehaviour
{
    public static GenerateDocument Instance { get; private set; }
    public static Action OnSpawnNextBatch;

    [Header("Document Prefabs")]
    [SerializeField] private List<GameObject> _leftDocuments = new List<GameObject>();
    [SerializeField] private List<GameObject> _rightDocuments = new List<GameObject>();

    [Header("Spawn Locations")]
    [SerializeField] private Transform _leftSpawnPoint;
    [SerializeField] private Transform _rightSpawnPoint;

    [Header("Parent Containers")]
    [SerializeField] private GameObject _leftParent;
    [SerializeField] private GameObject _rightParent;

    private static int _currentIndex = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Sync the dataset index with the current scene name
        MainDataset.CheckDay();

        // Then proceed with your normal spawning logic
        SpawnBatch();
    }

    private void OnEnable() => OnSpawnNextBatch += IncrementAndSpawn;
    private void OnDisable() => OnSpawnNextBatch -= IncrementAndSpawn;

    private void IncrementAndSpawn()
    {
        _currentIndex++;
        SpawnBatch();
    }

    public void SpawnBatch()
    {

        // 1. Existing check: Ensure we haven't run out of physical Prefabs in the Inspector
        if (_currentIndex >= _rightDocuments.Count || _currentIndex >= _leftDocuments.Count)
        {
            Debug.LogWarning("GenerateDocument: No more prefabs in list!");
            return;
        }

        // 2. NEW check: Ensure the MainDataset still has text for this specific day/row
        // We check if the current Document index is beyond the size of the current Group (Day)
        if (!MainDataset.HasMoreDocumentsInCurrentDay())
        {
            Debug.Log("<color=orange>GenerateDocument:</color> All text entries for this day have been exhausted. Stopping spawn.");
            return;
        }

        // --- SPAWN RIGHT DOCUMENT ---
        // 1. Instantiate in world space (null parent)
        GameObject rightDoc = Instantiate(_rightDocuments[_currentIndex], _rightSpawnPoint.position, _rightSpawnPoint.rotation);

        // 2. Force Local Scale BEFORE parenting
        rightDoc.transform.localScale = Vector3.one;

        // 3. Set Parent
        rightDoc.transform.SetParent(_rightParent.transform);

        // 4. Force Local Z to 0 (ensures it doesn't spawn behind the desk)
        Vector3 rightLocalPos = rightDoc.transform.localPosition;
        rightDoc.transform.localPosition = new Vector3(rightLocalPos.x, rightLocalPos.y, 0f);

        rightDoc.name = $"Doc_R_{_currentIndex}";


        // --- SPAWN LEFT DOCUMENT ---
        // 1. Instantiate in world space
        GameObject leftDoc = Instantiate(_leftDocuments[_currentIndex], _leftSpawnPoint.position, _leftSpawnPoint.rotation);

        // 2. Force Local Scale
        leftDoc.transform.localScale = Vector3.one;

        // 3. Set Parent
        leftDoc.transform.SetParent(_leftParent.transform);

        leftDoc.name = $"Doc_L_{_currentIndex}";


        // --- LINKING ---
        SwitchDocBetweenScreens rightSwitch = rightDoc.GetComponent<SwitchDocBetweenScreens>();
        SwitchDocBetweenScreens leftSwitch = leftDoc.GetComponent<SwitchDocBetweenScreens>();

        if (rightSwitch != null && leftSwitch != null)
        {
            rightSwitch.SetPairDoc(leftDoc);
            leftSwitch.SetPairDoc(rightDoc);
        }
    }


    public static int GetCurrentIndex() => _currentIndex;
}