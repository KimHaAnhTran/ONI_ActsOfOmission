using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public class GenerateDocument : MonoBehaviour
{
    public static GenerateDocument Instance { get; private set; }
    public static Action OnSpawnNextBatch;

    [Header("Master Prefab Pools (Flat Lists)")]
    [SerializeField] private List<GameObject> _leftDocuments = new List<GameObject>();
    [SerializeField] private List<GameObject> _rightDocuments = new List<GameObject>();

    [Header("Spawn Locations")]
    [SerializeField] private Transform _leftSpawnPoint;
    [SerializeField] private Transform _rightSpawnPoint;

    [Header("Parent Containers")]
    [SerializeField] private GameObject _leftParent;
    [SerializeField] private GameObject _rightParent;

    private List<List<GameObject>> _leftBatches = new List<List<GameObject>>();
    private List<List<GameObject>> _rightBatches = new List<List<GameObject>>();

    private static int _localDocIndex = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        OrganizePrefabsIntoBatches();
    }

    private void Start()
    {
        MainDataset.CheckDay();
        _localDocIndex = 0;
    }

    private void OnEnable() => OnSpawnNextBatch += IncrementAndSpawn;
    private void OnDisable() => OnSpawnNextBatch -= IncrementAndSpawn;

    private void OrganizePrefabsIntoBatches()
    {
        int prefabPointer = 0;
        for (int i = 0; i < MainDataset.DocumentGroups.Count; i++)
        {
            List<GameObject> currentLeftDay = new List<GameObject>();
            List<GameObject> currentRightDay = new List<GameObject>();
            int docsInThisDay = MainDataset.DocumentGroups[i].Count;

            for (int j = 0; j < docsInThisDay; j++)
            {
                if (prefabPointer < _leftDocuments.Count)
                {
                    currentLeftDay.Add(_leftDocuments[prefabPointer]);
                    currentRightDay.Add(_rightDocuments[prefabPointer]);
                    prefabPointer++;
                }
            }
            _leftBatches.Add(currentLeftDay);
            _rightBatches.Add(currentRightDay);
        }
    }

    private void IncrementAndSpawn()
    {
        // 1. Spawn the current index (starts at 0)
        SpawnBatch();

        // 2. Increment AFTER spawning so the NEXT call gets the next index
        _localDocIndex++;
    }

    public void SpawnBatch()
    {
        int currentDay = MainDataset.GetGroupIndex();

        if (!MainDataset.HasMoreDocumentsInCurrentDay())
        {
            Debug.Log("<color=orange>GenerateDocument:</color> All documents for this day have been spawned.");
            GameManager.Instance.StartEndOfDayTransition();
            return;
        }

        if (currentDay >= _leftBatches.Count || _localDocIndex >= _leftBatches[currentDay].Count)
        {
            Debug.LogError($"GenerateDocument: No Prefab assigned for Day {currentDay} at Index {_localDocIndex}!");
            return;
        }

        // --- SPAWN RIGHT DOCUMENT ---
        // 1. Instantiate in world space (null parent)
        GameObject rightPrefab = _rightBatches[currentDay][_localDocIndex];
        GameObject rightDoc = Instantiate(rightPrefab, _rightSpawnPoint.position, _rightSpawnPoint.rotation);

        // 2. Force Local Scale BEFORE parenting
        rightDoc.transform.localScale = Vector3.one;

        // 3. Set Parent
        rightDoc.transform.SetParent(_rightParent.transform);

        // 4. Force Local Z to 0
        Vector3 rightLocalPos = rightDoc.transform.localPosition;
        rightDoc.transform.localPosition = new Vector3(rightLocalPos.x, rightLocalPos.y, 0f);

        rightDoc.name = $"Doc_R_Day{currentDay + 1}_{_localDocIndex}";


        // --- SPAWN LEFT DOCUMENT ---
        // 1. Instantiate in world space
        GameObject leftPrefab = _leftBatches[currentDay][_localDocIndex];
        GameObject leftDoc = Instantiate(leftPrefab, _leftSpawnPoint.position, _leftSpawnPoint.rotation);

        // 2. Force Local Scale
        leftDoc.transform.localScale = Vector3.one;

        // 3. Set Parent
        leftDoc.transform.SetParent(_leftParent.transform);

        // 4. Local Z Cleanup (Matches right doc logic)
        Vector3 leftLocalPos = leftDoc.transform.localPosition;
        leftDoc.transform.localPosition = new Vector3(leftLocalPos.x, leftLocalPos.y, 0f);

        leftDoc.name = $"Doc_L_Day{currentDay + 1}_{_localDocIndex}";


        // --- LINKING ---
        SwitchDocBetweenScreens rightSwitch = rightDoc.GetComponent<SwitchDocBetweenScreens>();
        SwitchDocBetweenScreens leftSwitch = leftDoc.GetComponent<SwitchDocBetweenScreens>();

        if (rightSwitch != null && leftSwitch != null)
        {
            rightSwitch.SetPairDoc(leftDoc);
            leftSwitch.SetPairDoc(rightDoc);
        }

        AudiopoolSFX.Instance.Play("SFX_PaperSlide");
    }

    public static int GetCurrentIndex() => _localDocIndex;
}