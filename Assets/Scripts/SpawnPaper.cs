using UnityEngine;

public class SpawnPaper : MonoBehaviour
{
    [SerializeField] private GameObject _paperPrefab;
    [SerializeField] private Transform _spawnPoint;

    private bool _hasSpawned = false;

    private void OnEnable()
    {
        TypewriterKey.OnCanTypeChanged += CheckSpawn;
    }

    private void OnDisable()
    {
        TypewriterKey.OnCanTypeChanged -= CheckSpawn;
    }

    private void CheckSpawn(bool canType)
    {
        if (canType && !_hasSpawned)
        {
            // Instantiate as a child of this object (transform)
            GameObject newPaper = Instantiate(_paperPrefab, transform);

            // Set the LOCAL position relative to the parent
            // We take the X and Y from the spawn point, but force Z to 0.7f
            newPaper.transform.localPosition = new Vector3(_spawnPoint.localPosition.x, _spawnPoint.localPosition.y, 0.7f);

            // Ensure rotation matches
            newPaper.transform.localRotation = _spawnPoint.localRotation;

            _hasSpawned = true;
        }
    }
}