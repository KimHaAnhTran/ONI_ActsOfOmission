using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageFlip : MonoBehaviour
{
    private enum BookState { Closed, Open }
    [SerializeField] private BookState _currentState = BookState.Closed;

    [Header("Sprite References (Parent)")]
    [SerializeField] private Sprite _bookClosedSprite;
    [SerializeField] private Sprite _bookOpenSprite;

    [Header("Sprite References (Flip)")]
    [SerializeField] private Sprite _flipRightSprite;
    [SerializeField] private Sprite _flipLeftSprite;

    [Header("Objects to Toggle")]
    [SerializeField] private List<GameObject> _pageElementsFlipped = new List<GameObject>();
    [SerializeField] private List<GameObject> _pageElementsDefault = new List<GameObject>();

    [Header("Parent Collider Settings")]
    [SerializeField] private Vector2 _parentOffsetClosed = new Vector2(0.7239926f, -0.131645f);
    [SerializeField] private Vector2 _parentSizeClosed = new Vector2(1.481171f, 1.93671f);
    [SerializeField] private Vector2 _parentOffsetOpen = new Vector2(-0.007555842f, -0.131645f);
    [SerializeField] private Vector2 _parentSizeOpen = new Vector2(2.944268f, 1.93671f);

    [Header("Flip Trigger Position Settings")]
    // These move the actual GameObject this script is attached to
    [SerializeField] private Vector3 _posClosed = new Vector3(1.45f, 0f, 0f); // Right side
    [SerializeField] private Vector3 _posOpen = new Vector3(-1.45f, 0f, 0f);  // Left side

    private SpriteRenderer _parentRenderer;
    private SpriteRenderer _flipRenderer;

    private BoxCollider2D _parentCollider;

    private void Awake()
    {
        // Cache components
        _parentRenderer = transform.parent.GetComponent<SpriteRenderer>();
        _flipRenderer = transform.GetComponent<SpriteRenderer>();
        _parentCollider = transform.parent.GetComponent<BoxCollider2D>();

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

        transform.localPosition = isOpen ? _posOpen : _posClosed;
        _flipRenderer.sprite = isOpen ? _flipLeftSprite : _flipRightSprite;


        // 4. Toggle Page Elements
        foreach (GameObject element in _pageElementsFlipped)
        {
            if (element != null)
            {
                element.SetActive(isOpen);
            }
        }

        foreach (GameObject element in _pageElementsDefault) {
            if (element != null) {
                element.SetActive(!isOpen);
            }
        }
    }
}
