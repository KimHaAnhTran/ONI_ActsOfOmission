using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchDocFromLeft : MonoBehaviour
{
    [SerializeField] private GameObject _pairDoc;
    private Vector2 _mousePosition;
    private float _switchPoint = 0.5895f;
    private bool _isDragging;
    private void Update()
    {
        _mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _isDragging = GetComponent<DragDrop>().IsDragging;
        CheckSwitchDoc();

    }


    private void CheckSwitchDoc()
    {
        if (_mousePosition.x > _switchPoint && _isDragging)
        {
            GetComponent<DragDrop>().IsDragging = false;
            transform.position = new Vector3(transform.position.x + 3f, transform.position.y + 3f, transform.position.z);
            _pairDoc.transform.position = new Vector3(_mousePosition.x, _mousePosition.y, _pairDoc.transform.position.z);
            _pairDoc.GetComponent<DragDrop>().IsDragging = true;
        }

    }
}
