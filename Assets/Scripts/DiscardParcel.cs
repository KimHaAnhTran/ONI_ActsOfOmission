using UnityEngine;
using System.Collections;

public class DiscardParcel : MonoBehaviour
{
    [Header("Position Settings")]
    [SerializeField] private Transform _yHidden;
    [SerializeField] private Transform _yVisible;
    [SerializeField] private float _hoverOffset = 0.5f;

    [Header("Animation Settings")]
    [SerializeField] private float _smoothTime = 0.15f;

    private float _currentTargetY;
    private float _currentVelocity;
    private bool _isActive = false;
    private bool _isMouseOver = false; // Tracks mouse cursor

    private void Awake()
    {
        transform.localPosition = new Vector3(transform.localPosition.x, _yHidden.position.y, transform.localPosition.z);
        _currentTargetY = _yHidden.position.y;
    }

    private void OnEnable()
    {
        TextType.OnCurrentDocumentFinished += PopIn;
        SendParcel.OnParcelProcessed += Hide; // Listen to SendParcel's event
    }

    private void OnDisable()
    {
        TextType.OnCurrentDocumentFinished -= PopIn;
        SendParcel.OnParcelProcessed -= Hide;
    }

    private void PopIn()
    {
        _isActive = true;
        _currentTargetY = _yVisible.position.y;
    }

    private void Hide()
    {
        _isActive = false;
        _currentTargetY = _yHidden.position.y;
    }

    private void OnMouseEnter() => _isMouseOver = true;
    private void OnMouseExit()
    {
        _isMouseOver = false;
        if (_isActive) _currentTargetY = _yVisible.position.y;
    }

    private void Update()
    {
        float newY = Mathf.SmoothDamp(transform.localPosition.y, _currentTargetY, ref _currentVelocity, _smoothTime);
        transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!_isActive || !other.CompareTag("Parcel")) return;

        DragDrop dd = other.GetComponent<DragDrop>();
        if (dd != null && dd.IsDragging)
        {
            if (_isMouseOver)
                _currentTargetY = _yVisible.position.y + _hoverOffset;
            else
                _currentTargetY = _yVisible.position.y;
        }
        else if (dd != null && !dd.IsDragging && _isMouseOver)
        {
            StartCoroutine(DiscardParcelRoutine(other.gameObject));
        }
    }

    private IEnumerator DiscardParcelRoutine(GameObject parcel)
    {
        SendParcel.OnParcelProcessed?.Invoke();

        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startPos = parcel.transform.position;
        Vector3 endPos = startPos + Vector3.down * 4f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            parcel.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            yield return null;
        }

        Destroy(parcel);
    }
}