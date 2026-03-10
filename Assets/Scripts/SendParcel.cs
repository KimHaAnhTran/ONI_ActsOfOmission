using UnityEngine;
using System.Collections;
using System;
using TMPro;

public class SendParcel : MonoBehaviour
{
    public static Action OnParcelProcessed; // Signal for both buttons to hide

    [Header("Position Settings")]
    [SerializeField] private Transform _yHidden;
    [SerializeField] private Transform _yVisible;
    [SerializeField] private float _hoverOffset = -0.5f;

    [Header("Animation Settings")]
    [SerializeField] private float _smoothTime = 0.15f;

    [Header("History Warning")]
    [SerializeField] private GameObject _textMeshPro;

    private float _currentTargetY;
    private float _currentVelocity;
    private bool _isActive = false;
    private bool _isMouseOver = false; // Tracks mouse cursor

    private void Awake()
    {
        transform.localPosition = new Vector3(transform.localPosition.x, _yHidden.position.y, transform.localPosition.z);
        _currentTargetY = _yHidden.position.y;
        _textMeshPro.GetComponent<TextMeshProUGUI>().text = "Your actions will affect history\r\nAre you <color=#FFD402>SURE</color>?";
        _textMeshPro.SetActive(false);
    }

    private void OnEnable()
    {
        TextType.OnCurrentDocumentFinished += PopIn;
        OnParcelProcessed += Hide; // Listen for the cleanup signal
    }

    private void OnDisable()
    {
        TextType.OnCurrentDocumentFinished -= PopIn;
        OnParcelProcessed -= Hide;
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
        _textMeshPro.SetActive(false);
    }

    // Unity built-in mouse tracking
    private void OnMouseEnter() => _isMouseOver = true;
    private void OnMouseExit()
    {
        _isMouseOver = false;
        if (_isActive) _currentTargetY = _yVisible.position.y; // Reset offset on exit
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

        // Only "Edge out" if BOTH the parcel is here AND the mouse is over this object
        if (dd != null && dd.IsDragging)
        {
            if (_isMouseOver)
                _currentTargetY = _yVisible.position.y + _hoverOffset;
            else
                _currentTargetY = _yVisible.position.y;
            _textMeshPro.SetActive(_isMouseOver);
        }
        else if (dd != null && !dd.IsDragging && _isMouseOver)
        {
            // Dropped on this specific button
            StartCoroutine(PullParcelRoutine(other.gameObject));
        }
    }

    private IEnumerator PullParcelRoutine(GameObject parcel)
    {
        // Fire global signal so BOTH buttons hide immediately
        OnParcelProcessed?.Invoke();

        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startPos = parcel.transform.position;
        Vector3 endPos = startPos + Vector3.up * 4f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            parcel.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            yield return null;
        }

        Destroy(parcel);
    }
}