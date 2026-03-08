using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchDocSize : MonoBehaviour
{
    [SerializeField] private GameObject _pairDoc;
    private Vector2 _mousePosition;
    [SerializeField] private bool _isRightDoc = false;
    private float _switchPoint = 0.0585f;
    private bool _isDragging;
    private void Update()
    {
        _mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _isDragging = GetComponent<DragDrop>().IsDragging;
        CheckSwitchDoc();

    }


    private void CheckSwitchDoc() {
        



    }
}
