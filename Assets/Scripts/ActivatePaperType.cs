using UnityEngine;
using TMPro;

public class ActivatePaperType : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _paperText; // Drag your TMPro object here
    private bool _hideOnStart = true;

    private void Awake()
    {
        // If not assigned in inspector, try to find it on this object
        if (_paperText == null)
            _paperText = GetComponent<TextMeshProUGUI>();

        // Initial State
        if (_hideOnStart && _paperText != null)
            _paperText.enabled = false;
    }

    private void OnEnable()
    {
        // Subscribe to the OnCanTypeChanged
        TypewriterKey.OnCanTypeChanged += ToggleTextVisibility;

        // Sync with the current state in case the event already fired
        ToggleTextVisibility(TypewriterKey.CanType);
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent errors
        TypewriterKey.OnCanTypeChanged -= ToggleTextVisibility;
    }

    private void ToggleTextVisibility(bool canType)
    {
        if (_paperText == null) return;
        _paperText.enabled = canType;

        // Optional: If you want the text to start empty when activated
        if (canType && string.IsNullOrEmpty(_paperText.text))
        {
            _paperText.text = "";
        }
    }
}