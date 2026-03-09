using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class HighlightRollOut : MonoBehaviour
{
    // Static so you can change the speed for ALL highlights in one place via the Inspector
    private static float _rollOutDuration = 0.3f;

    private Image _highlightImage;
    private float _targetFill = 1f;
    private float _currentVelocity = 1f; // Used for SmoothDamp
    private bool _hasReachedTarget = false;

    // Adjust the static duration from the inspector of any instance
    private float _sharedDuration;

    private void Awake()
    {
        _highlightImage = GetComponent<Image>();
        _highlightImage.fillAmount = 0f; // Start hidden
        _sharedDuration = _rollOutDuration;
    }

    private void OnEnable()
    {
        _highlightImage.fillAmount = 0f;
        _hasReachedTarget = false;
    }

    private void Update()
    {
        if (_hasReachedTarget) return;
        // Smoothly transition the fillAmount toward the target
        _highlightImage.fillAmount = Mathf.SmoothDamp(
            _highlightImage.fillAmount,
            _targetFill,
            ref _currentVelocity,
            _sharedDuration
        );

        // Because SmoothDamp never reaches its target, we skip ahead until it reaches it
        // For sake of memory storage
        if (Mathf.Abs(_highlightImage.fillAmount - _targetFill) <= 0.001f) {
            _highlightImage.fillAmount = _targetFill;
            _hasReachedTarget = true;
        }
    }

}