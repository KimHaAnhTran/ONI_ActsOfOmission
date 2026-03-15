using UnityEngine;

public class TextHover : MonoBehaviour
{
    [SerializeField] private GameObject _highlights;
    [SerializeField] private Texture2D _handCursorTexture;

    private bool _isHover = false;
    private bool _isLocked = false;

    private void Start() => _highlights.SetActive(false);

    private void OnEnable()
    {
        // Listen for the static event from TextType
        TextType.OnCurrentDocumentFinished += HandleDocumentFinished;
    }

    private void OnDisable()
    {
        TextType.OnCurrentDocumentFinished -= HandleDocumentFinished;
    }

    private void HandleDocumentFinished()
    {
        // If this doc was the one active, lock it permanently
        if (_isHover) LockHighlight();
    }

    public void LockHighlight()
    {
        _isLocked = true;
        _highlights.SetActive(true);
        // Return cursor to normal since we can't click it again
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    public void OnMouseEnter()
    {
        Debug.Log("Is Locked?" + _isLocked);
        if (_isLocked) return;

        _highlights.SetActive(true);
        _isHover = true;
        Cursor.SetCursor(_handCursorTexture, Vector2.zero, CursorMode.Auto);
    }

    public void OnMouseExit()
    {
        _isHover = false;
        if (_isLocked) return; // THE FIX: If locked, stay visible!

        _highlights.SetActive(false);
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}