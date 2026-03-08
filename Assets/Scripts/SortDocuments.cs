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
        _documents.Remove(this.gameObject); // Remove from list to re-insert at top
        CreateShadowClone();
        _documents.Add(this._shadow); // Add shadow just below host
        _documents.Add(this.gameObject); // Add host to the very top
        updateHierarchy();
    }

    private void OnMouseDrag()
    {
        // Keep shadow following host during drag
        UpdateShadowPosition(); 
    }

    private void OnMouseUp()
    {
        // Clean up list & remove shadow
        _documents.Remove(_shadow);
        Destroy(_shadow);
    }

    private void updateHierarchy()
    {
        float zIndex = 0f;

        // Apply new Z-depths based on list order
        foreach (GameObject child in _documents)
        {
            zIndex -= 0.1f;
            Transform childPos = child.transform;
            childPos.localPosition = new Vector3(childPos.localPosition.x, childPos.localPosition.y, zIndex);
        }
    }

    private void CreateShadowClone()
    {
        // 1. Create the clone with a slight offset
        Vector3 offset = new Vector3(-.1f, -.1f, 0f);
        _shadow = Instantiate(this.gameObject, transform.position + offset, transform.rotation, transform.parent);
        _shadow.name = "Shadow";

        // 2. Strip scripts to prevent clone logic from running
        MonoBehaviour[] scripts = _shadow.GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            Destroy(script);
        }

        // 3. Disable physics/interaction on shadow
        BoxCollider2D collider = _shadow.GetComponent<BoxCollider2D>();
        if (collider != null) Destroy(collider);

        // 4. Visual styling for the "shadow" effect
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
}