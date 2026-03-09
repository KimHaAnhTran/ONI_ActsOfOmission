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

                BoxCollider2D[] boxCollider2Ds = GetComponentsInChildren<BoxCollider2D>();

                foreach (BoxCollider2D collider in boxCollider2Ds) {
                    collider.enabled = false; // Player no longer click on box collider
                }
            }
        }
    }
}