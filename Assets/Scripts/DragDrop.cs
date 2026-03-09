using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class DragDrop : MonoBehaviour
{
    // --- Private Fields ---
    private bool _isDragging = false;
    private Vector2 _mousePosition;
    private Vector2 _dragOffset;

    public bool IsDragging {
        get { return _isDragging; }
        set { _isDragging = value; }
    }
    private void Update()
    {
        // Convert mouse screen space to world coordinates
        _mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        UpdateObjectPosition();
        if (Input.GetMouseButtonUp(0)) {
            _isDragging = false;
        }
    }

    private void OnMouseDown()
    {
        _mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // Calculate distance between mouse and object center to prevent snapping
        _dragOffset = _mousePosition - (Vector2)transform.position;

        _isDragging = true;
    }

    private void OnMouseUp()
    {
        _isDragging = false; // Stop dragging on release
    }

    public void StartManualDrag(Vector2 mousePos) {
        _dragOffset = mousePos - (Vector2)transform.position;
        _isDragging = true;
    }

    private void UpdateObjectPosition()
    {
        if (!_isDragging) return;

        // Apply movement while maintaining original Z depth
        float newXPos = _mousePosition.x - _dragOffset.x;
        float newYPos = _mousePosition.y - _dragOffset.y;
        transform.position = new Vector3(newXPos, newYPos, transform.position.z);
    }

}