using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SortDocuments : MonoBehaviour
{
    private static List<GameObject> _documents = new List<GameObject>();

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
        // Move clicked object to the end of the list (top of stack)
        _documents.Remove(this.gameObject);
        _documents.Add(this.gameObject);
        updateHierarchy();

    }

    private void updateHierarchy()
    {
        float zIndex = 0f;

        // Re-sort all documents' Z positions based on list order
        foreach (GameObject child in _documents)
        {
            zIndex -= 0.1f;
            Transform childPos = child.transform;
            childPos.localPosition = new Vector3(childPos.localPosition.x, childPos.localPosition.y, zIndex);
        }
    }
}