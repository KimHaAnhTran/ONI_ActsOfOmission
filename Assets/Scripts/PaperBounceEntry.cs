using UnityEngine;

public class PaperBounceEntry : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Transform _targetLocal; // Set the desired local Y goal in Inspector
    [SerializeField] private float _smoothTime = 0.3f;

    [Header("Bounce Settings")]
    [SerializeField] private float _bounceAmplitude = 0.1f;
    [SerializeField] private float _bounceFrequency = 15f;
    [SerializeField] private float _bounceDuration = 1.0f;

    private float _yVelocity = 0.2f;
    private float _startTime;

    private void Start()
    {
        _startTime = Time.time;

        // Safety: Ensure we start at 0.7f Z just in case
        Vector3 pos = transform.localPosition;
        pos.z = 0.7f;
        transform.localPosition = pos;
    }

    private void Update()
    {
        // 1. Smoothly transition to the target LOCAL Y position
        float currentY = transform.localPosition.y;
        float newY = Mathf.SmoothDamp(currentY, _targetLocal.position.y, ref _yVelocity, _smoothTime);

        // 2. Add the mathematical bounce
        float elapsed = Time.time - _startTime;

        if (elapsed < _bounceDuration)
        {
            float decay = Mathf.Clamp01(1 - (elapsed / _bounceDuration));
            float bounceOffset = Mathf.Sin(elapsed * _bounceFrequency) * _bounceAmplitude * decay;
            newY += bounceOffset;
        }

        // Apply new Y while FORCING local Z to stay at 0.7f
        transform.localPosition = new Vector3(transform.localPosition.x, newY, 0.7f);
    }
}