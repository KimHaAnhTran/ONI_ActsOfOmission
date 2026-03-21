using UnityEngine;

public class SwitchDocBetweenScreens : MonoBehaviour
{
    [SerializeField] private GameObject _pairDoc;
    [SerializeField] private bool _isSmallDoc;

    public GameObject PairDoc {
        get { return _pairDoc; }
    }

    private DragDrop _dragDrop;
    private DragDrop _pairDragDrop;
    private float _switchPoint = 0.5895f;

    private void Awake()
    {
        // Cache components once to avoid calling GetComponent every frame
        _dragDrop = GetComponent<DragDrop>();
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

    public void SetPairDoc(GameObject pairDoc)
    {
        _pairDoc = pairDoc;
        if (_pairDoc != null)
        {
            // Cache the DragDrop component right then and there
            // Otherwise _pairDragDrop will be null when the Switch happens.
            _pairDragDrop = _pairDoc.GetComponent<DragDrop>();
        }
    }

    private void Switch(Vector2 mousePos)
    {
        // 1. Stop dragging this one
        _dragDrop.IsDragging = false;

        // NEW: If the small doc has a shadow, clear it before switching
        SortDocuments smallSorter = GetComponent<SortDocuments>();
        if (smallSorter != null) smallSorter.ClearShadow();

        // 2. Teleport this one away
        transform.position += new Vector3(50f, 0, 0);

        // 3. Move the pair to exactly where the mouse is
        _pairDoc.transform.position = new Vector3(mousePos.x, mousePos.y, _pairDoc.transform.position.z);

        // 4. Force the pair to start dragging
        if (_pairDragDrop == null) _pairDragDrop = _pairDoc.GetComponent<DragDrop>();

        if (_pairDragDrop != null)
        {
            _pairDragDrop.StartManualDrag(mousePos);

            // Use a "Silent" sort that doesn't create shadows
            SortDocuments bigSorter = _pairDoc.GetComponent<SortDocuments>();
            if (bigSorter != null)
            {
                // We call a new public method we'll add to SortDocuments
                bigSorter.ForceToTop();
            }
        }
    }
}