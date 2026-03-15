using UnityEngine;
using System.Collections.Generic;

public class BookmarkFlip : MonoBehaviour
{
    private enum BookState { Closed, Open }
    [SerializeField] private BookState _currentState = BookState.Closed;

    [Header("Sprite References (Parent)")]
    [SerializeField] private Sprite _bookClosedSprite;
    [SerializeField] private Sprite _bookOpenSprite;

    [Header("Objects to Toggle")]
    [SerializeField] private List<GameObject> _pageElements = new List<GameObject>();

    [Header("Parent Collider Settings")]
    [SerializeField] private Vector2 _parentOffsetClosed = new Vector2(0.7239926f, -0.131645f);
    [SerializeField] private Vector2 _parentSizeClosed = new Vector2(1.481171f, 1.93671f);
    [SerializeField] private Vector2 _parentOffsetOpen = new Vector2(-0.007555842f, -0.131645f);
    [SerializeField] private Vector2 _parentSizeOpen = new Vector2(2.944268f, 1.93671f);

    [Header("Bookmark Collider Settings")]
    [SerializeField] private Vector2 _bookmarkOffsetClosed = new Vector2(0f, 0f);
    [SerializeField] private Vector2 _bookmarkSizeClosed = new Vector2(1f, 1f);
    [SerializeField] private Vector2 _bookmarkOffsetOpen = new Vector2(0f, -0.9736806f);
    [SerializeField] private Vector2 _bookmarkSizeOpen = new Vector2(1f, 1.081994f);

    private SpriteRenderer _parentRenderer;
    private BoxCollider2D _parentCollider;
    private BoxCollider2D _bookmarkCollider;

    private void Awake()
    {
        // Cache components
        _parentRenderer = transform.parent.GetComponent<SpriteRenderer>();
        _parentCollider = transform.parent.GetComponent<BoxCollider2D>();
        _bookmarkCollider = GetComponent<BoxCollider2D>();

        // Ensure the book starts in the correct state
        UpdateVisualsAndPhysics();
    }

    private void OnMouseDown()
    {
        ToggleBook();
    }

    private void ToggleBook()
    {
        // Flip the enum state
        _currentState = (_currentState == BookState.Closed) ? BookState.Open : BookState.Closed;

        UpdateVisualsAndPhysics();
    }

    private void UpdateVisualsAndPhysics()
    {
        bool isOpen = (_currentState == BookState.Open);

        // 1. Swap Parent Sprite
        if (_parentRenderer != null)
        {
            _parentRenderer.sprite = isOpen ? _bookOpenSprite : _bookClosedSprite;
        }

        // 2. Update Parent Collider
        if (_parentCollider != null)
        {
            _parentCollider.offset = isOpen ? _parentOffsetOpen : _parentOffsetClosed;
            _parentCollider.size = isOpen ? _parentSizeOpen : _parentSizeClosed;
        }

        // 3. Update Bookmark Collider (This object)
        if (_bookmarkCollider != null)
        {
            // Use the original X for offset/size as requested
            float finalOffsetX = isOpen ? _bookmarkCollider.offset.x : _bookmarkOffsetClosed.x;
            float finalSizeX = isOpen ? _bookmarkCollider.size.x : _bookmarkSizeClosed.x;

            _bookmarkCollider.offset = new Vector2(finalOffsetX, isOpen ? _bookmarkOffsetOpen.y : _bookmarkOffsetClosed.y);
            _bookmarkCollider.size = new Vector2(finalSizeX, isOpen ? _bookmarkSizeOpen.y : _bookmarkSizeClosed.y);
        }

        // 4. Toggle Page Elements
        foreach (GameObject element in _pageElements)
        {
            if (element != null)
            {
                element.SetActive(isOpen);
            }
        }
    }
}