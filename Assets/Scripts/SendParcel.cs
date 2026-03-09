using UnityEngine;
using System.Collections;

public class SendParcel : MonoBehaviour
{
    [Header("Position Settings")]
    [SerializeField] private Transform _yHidden;  // y1
    [SerializeField] private Transform _yVisible; // y2
    [SerializeField] private float _hoverOffset = -0.5f; // How much it edges down

    [Header("Animation Settings")]
    [SerializeField] private float _smoothTime = 0.15f;
    [SerializeField] private float _popDuration = 0.5f;

    private float _currentTargetY;
    private float _currentVelocity;
    private bool _isActive = false;

    private void Awake()
    {
        // Start hidden
        transform.localPosition = new Vector3(transform.localPosition.x, _yHidden.position.y, transform.localPosition.z);
        _currentTargetY = _yHidden.position.y;
    }

    private void OnEnable() => TextType.OnCurrentDocumentFinished += PopIn;
    private void OnDisable() => TextType.OnCurrentDocumentFinished -= PopIn;

    private void PopIn()
    {
        _isActive = true;
        _currentTargetY = _yVisible.position.y;
        // Optional: Trigger a bounce here or use SmoothDamp in Update
    }

    private void Update()
    {
        float newY = Mathf.SmoothDamp(transform.localPosition.y, _currentTargetY, ref _currentVelocity, _smoothTime);
        transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!_isActive || !other.CompareTag("Parcel")) return;

        Debug.Log(other.CompareTag("Parcel"));

        DragDrop dd = other.GetComponent<DragDrop>();
        if (dd != null && dd.IsDragging)
        {
            // Edge down while hovering with parcel
            _currentTargetY = _yVisible.position.y + _hoverOffset;
        }
        else if (dd != null && !dd.IsDragging)
        {
            // Logic for dropping the parcel
            StartCoroutine(PullParcelRoutine(other.gameObject));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Parcel"))
        {
            _currentTargetY = _yVisible.position.y;
        }
    }

    private IEnumerator PullParcelRoutine(GameObject parcel)
    {
        _isActive = false; // Prevent multiple triggers
        _currentTargetY = _yHidden.position.y; // Move Send button back up

        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startPos = parcel.transform.position;
        Vector3 endPos = startPos + Vector3.up * 10f; // Pull up out of screen

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            parcel.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            yield return null;
        }

        Destroy(parcel);
    }
}