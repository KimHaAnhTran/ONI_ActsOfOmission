using UnityEngine;

public class SpawnPaper : MonoBehaviour
{
    [SerializeField] private GameObject _paperPrefab;
    [SerializeField] private Transform _spawnPoint;

    private bool _hasSpawned = false;

    private void OnEnable()
    {
        // Subscribe to existing typing event
        TypewriterKey.OnCanTypeChanged += CheckSpawn;

        // Find the GameManager by tag and subscribe to the spawn action
        GameObject managerObj = GameObject.FindWithTag("GameManager");
        if (managerObj != null)
        {
            // We access the static Action on the GenerateDocument class
            GenerateDocument.OnSpawnNextBatch += ResetSpawnFlag;
        }
    }

    private void OnDisable()
    {
        TypewriterKey.OnCanTypeChanged -= CheckSpawn;

        // Unsubscribe to prevent memory leaks or null reference errors
        GenerateDocument.OnSpawnNextBatch -= ResetSpawnFlag;
    }

    private void ResetSpawnFlag()
    {
        // When a new batch starts, this object is allowed to spawn a paper again
        _hasSpawned = false;
    }

    private void CheckSpawn(bool canType)
    {
        if (canType && !_hasSpawned)
        {
            // Instantiate as a child of this object (transform)
            GameObject newPaper = Instantiate(_paperPrefab, transform);

            // Set the LOCAL position relative to the parent
            // Take X/Y from spawn point, force Z to 0.7f
            newPaper.transform.localPosition = new Vector3(_spawnPoint.localPosition.x, _spawnPoint.localPosition.y, 0.7f);

            // Ensure rotation matches
            newPaper.transform.localRotation = _spawnPoint.localRotation;

            _hasSpawned = true;
        }
    }
}