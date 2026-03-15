using UnityEngine;
using System.Collections;

public class DocumentEntry : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float _dropDistance = 0.3f;
    [SerializeField] private float _duration = 0.6f;
    [SerializeField] private AnimationCurve _dropCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine _entryCoroutine;
    private Vector3 _startPos;
    private Vector3 _targetPos;

    private void Start()
    {
        // Define the movement relative to the spawn point
        _startPos = transform.localPosition;
        _targetPos = _startPos + new Vector3(0, -_dropDistance, 0);

        // Start the sliding animation
        _entryCoroutine = StartCoroutine(SlideInRoutine());
    }

    private IEnumerator SlideInRoutine()
    {
        float elapsed = 0f;

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / _duration;

            // Use the curve for smoother acceleration/deceleration
            float curvePercent = _dropCurve.Evaluate(percent);

            transform.localPosition = Vector3.Lerp(_startPos, _targetPos, curvePercent);
            yield return null;
        }

        _entryCoroutine = null;
    }

    // This stops the movement as soon as the player interacts with it
    private void OnMouseDown()
    {
        StopEntry();
    }

    public void StopEntry()
    {
        if (_entryCoroutine != null)
        {
            StopCoroutine(_entryCoroutine);
            _entryCoroutine = null;

            // Optional: Snap to target if you want it to finish, 
            // or just leave it where it was grabbed.
            // transform.localPosition = _targetPos; 
        }

        // Disable the script so it doesn't try to run again
        this.enabled = false;
    }
}