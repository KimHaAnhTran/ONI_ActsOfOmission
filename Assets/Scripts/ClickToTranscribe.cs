using UnityEngine;

public class ClickToTranscribe : MonoBehaviour
{
    private bool _alreadyTranscribed = false;
    private TextHover _hoverScript;

    private void Awake()
    {
        _hoverScript = GetComponent<TextHover>();
    }

    private void OnMouseDown()
    {
        if (!_alreadyTranscribed)
        {
            TypewriterKey.CanType = true;
            _alreadyTranscribed = true;

            // Tell the hover script to lock the highlights ON
            if (_hoverScript != null)
            {
                _hoverScript.LockHighlight();
            }
        }
    }
}