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
        Debug.Log("Is already transcribed: " + _alreadyTranscribed);

        if (!_alreadyTranscribed)
        {
            TypewriterKey.CanType = true;
            _alreadyTranscribed = true;

            // 1. Lock highlights on the current document
            if (_hoverScript != null)
            {
                _hoverScript.LockHighlight();
            }

            // 2. Find every object in the scene tagged "HighlightText"
            GameObject[] highlightObjects = GameObject.FindGameObjectsWithTag("HighlightText");

            foreach (GameObject obj in highlightObjects)
            {
                // 3. Get all colliders on the object and its children
                BoxCollider2D[] colliders = obj.GetComponentsInChildren<BoxCollider2D>();

                foreach (BoxCollider2D collider in colliders)
                {
                    collider.enabled = false; // Disable interaction globally
                }
            }
        }
    }
}