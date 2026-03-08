using UnityEngine;

public class SwtichDocBetweenScreens : MonoBehaviour
{
    [SerializeField] private GameObject _pairDoc;
    [SerializeField] private bool _isSmallDoc;

    private DragDrop _dragDrop;
    private DragDrop _pairDragDrop;
    private float _switchPoint = 0.5895f;

    private void Awake()
    {
        // Cache components once to avoid calling GetComponent every frame
        _dragDrop = GetComponent<DragDrop>();
        if (_pairDoc != null) _pairDragDrop = _pairDoc.GetComponent<DragDrop>();
    }

    private void Update()
    {
        // Only run logic if we are actually dragging
        if (!_dragDrop.IsDragging) return;

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Boolean logic to decide the "is past threshold" state
        bool crossedThreshold = _isSmallDoc ? (mousePosition.x < _switchPoint) : (mousePosition.x > _switchPoint);

        if (crossedThreshold)
        {
            Switch(mousePosition);
        }
    }

    private void Switch(Vector2 mousePos)
    {
        _dragDrop.IsDragging = false;

        // Teleport current object away
        transform.position += new Vector3(3f, 3f, 0);

        // Hand over the drag to the pair
        _pairDoc.transform.position = new Vector3(mousePos.x, mousePos.y, _pairDoc.transform.position.z);
        if (_pairDragDrop != null) _pairDragDrop.IsDragging = true;
    }
}