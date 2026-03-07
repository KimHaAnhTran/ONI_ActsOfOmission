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
        foreach (Transform child in transform.parent) {
            zIndex -= 0.1f;
            _documents.Add(child.gameObject);
            child.localPosition = new Vector3(child.localPosition.x, child.localPosition.y, zIndex);

        }
    }
    private void OnMouseDown()
    {
        _documents.Remove(this.gameObject);
        _documents.Add(this.gameObject);
        updateHierarchy();

        int documentsCount = _documents.Count;
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, documentsCount * -.1f);
    }


    //Code is so much shorter than last time I did it!!!
    private void updateHierarchy() {
        float zIndex = 0f;

        foreach (GameObject child in _documents)
        {
            zIndex -= 0.1f;
            Transform childPos = child.transform;
            childPos.localPosition = new Vector3(childPos.localPosition.x, childPos.localPosition.y, zIndex);
        }

    }
}
