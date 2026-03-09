using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class HighlightRollOut : MonoBehaviour
{
    private static float _rollOutDuration = 0.3f;
    private Image _highlightImage;
    private float _targetFill = 1f;
    private float _currentVelocity = 1f;
    private bool _hasReachedTarget = false;

    private void Awake()
    {
        _highlightImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        // Reset every time the object is turned on (unless already locked)
        _highlightImage.fillAmount = 0f;
        _currentVelocity = 1f;
        _hasReachedTarget = false;
    }

    private void Update()
    {
        if (_hasReachedTarget) return;

        _highlightImage.fillAmount = Mathf.SmoothDamp(
            _highlightImage.fillAmount,
            _targetFill,
            ref _currentVelocity,
            _rollOutDuration
        );

        if (Mathf.Abs(_highlightImage.fillAmount - _targetFill) <= 0.001f)
        {
            _highlightImage.fillAmount = _targetFill;
            _hasReachedTarget = true;
        }
    }
}