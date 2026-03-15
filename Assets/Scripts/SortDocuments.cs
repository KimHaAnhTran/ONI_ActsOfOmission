using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SortDocuments : MonoBehaviour
{
    private static List<GameObject> _documents = new List<GameObject>();
    private GameObject _shadow;
    private const float CHILD_Z_OFFSET = -0.001f;

    private void Start()
    {
        _documents.Clear();
        float zIndex = 0f;
        // Initialize list and stack children along the Z axis
        foreach (Transform child in transform.parent)
        {
            zIndex -= 0.1f;
            _documents.Add(child.gameObject);
            child.localPosition = new Vector3(child.localPosition.x, child.localPosition.y, zIndex);
        }
    }

    private void OnMouseDown()
    {
        OnClick();
    }

    public void OnClick() {
        _documents.Remove(this.gameObject); // Remove from list to re-insert at top
        CreateShadowClone();
        _documents.Add(this._shadow); // Add shadow just below host
        _documents.Add(this.gameObject); // Add host to the very top
        UpdateHierarchy();
    }

    private void OnMouseDrag()
    {
        // Keep shadow following host during drag
        UpdateShadowPosition(); 
    }

    private void OnMouseUp()
    {
        
        ClearShadow();
    }

    public void ClearShadow()
    {
        // Clean up list & remove shadow
        _documents.Remove(_shadow);
        Destroy(_shadow);

    }

    private void UpdateHierarchy()
    {
        float zIndex = 0f;

        // 1. Sort the main documents (the "Rows")
        foreach (GameObject doc in _documents)
        {
            if (doc == null) continue;

            zIndex -= 0.1f;
            doc.transform.localPosition = new Vector3(doc.transform.localPosition.x, doc.transform.localPosition.y, zIndex);

            // 2. Sort all nested children for this specific document
            UpdateNestedChildrenZ(doc.transform, zIndex);
        }
    }

    private void UpdateNestedChildrenZ(Transform parent, float parentZ)
    {
        foreach (Transform child in parent)
        {
            // We want the child to be slightly "more negative" (closer to camera) than its parent
            float childZ = CHILD_Z_OFFSET;

            child.localPosition = new Vector3(child.localPosition.x, child.localPosition.y, childZ);

            // 3. Recurse! If this child has its own children (like a signature on a stamp)
            if (child.childCount > 0)
            {
                UpdateNestedChildrenZ(child, childZ);
            }
        }
    }

    private void CreateShadowClone()
    {
        // Create the clone with a slight offset
        Vector3 offset = new Vector3(-.1f, -.1f, 0f);
        _shadow = Instantiate(this.gameObject, transform.position + offset, transform.rotation, transform.parent);
        _shadow.name = "Shadow";

        // Strip scripts to prevent clone logic from running
        MonoBehaviour[] scripts = _shadow.GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            Destroy(script);
        }

        // Remove all children from shadow
        foreach (Transform child in _shadow.transform)
        {
            Destroy(child.gameObject);
        }

        // Disable physics/interaction on shadow
        BoxCollider2D collider = _shadow.GetComponent<BoxCollider2D>();
        if (collider != null) Destroy(collider);

        // Visual styling for the "shadow" effect
        SpriteRenderer sr = _shadow.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = new Color(0f, 0f, 0f, 0.8f);
        }
    }

    private void UpdateShadowPosition()
    {
        // Maintain relative offset and a slight Z-buffer while dragging
        Vector3 offset = new Vector3(-.1f, -.1f, .05f);
        _shadow.transform.position = transform.position + offset;
    }

    private void OnDestroy()
    {
        // Check if the list contains this object before removing to avoid errors
        if (_documents.Contains(this.gameObject))
        {
            _documents.Remove(this.gameObject);
        }

        // Also clean up the shadow if the parcel is destroyed mid-drag
        if (_shadow != null)
        {
            Destroy(_shadow);
        }

    }
}