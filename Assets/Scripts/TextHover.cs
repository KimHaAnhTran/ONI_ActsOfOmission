using UnityEngine;

public class TextHover : MonoBehaviour
{
    [SerializeField] private GameObject _highlights;
    private bool _isHover = false;
    private bool _isPressed = false;

    private Texture2D _handCursorTexture; // Defaults null, you have to [SerializeField] a hand symbol
                                          // Because Unity doesn't have a hand cursor like web

    public bool IsHover
    {
        get { return _isHover; }
        set { _isHover = value; }
    }

    private void Start()
    {
        _highlights.SetActive(false);
    }

    public void OnMouseEnter()
    {
        _highlights.SetActive(true);
        _isHover = true;
        _isPressed = TypewriterKey.CanType;

        // Change to Hand Cursor
        // If _handCursorTexture is null, Unity usually defaults to the OS pointer
        // For a true "Hand" look, it's best to provide a small 32x32 UI sprite
        Cursor.SetCursor(_handCursorTexture, Vector2.zero, CursorMode.Auto);
    }

    public void OnMouseExit()
    {
        // Reset to default arrow
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        if (_isPressed) return;
        _highlights.SetActive(false);
        _isHover = false;
    }
}