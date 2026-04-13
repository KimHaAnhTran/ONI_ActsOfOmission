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

    [Header("Screen Boundaries (Global)")]
    // Set to the exact global coordinates you provided
    private float _minX = -2.47f;
    private float _maxX = 2.44f;
    private float _minY = -1.43f;
    private float _maxY = 1.42286f;
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
        AudiopoolSFX.Instance.Play("SFX_PaperDragDrop");

        _mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // Calculate distance between mouse and object center to prevent snapping
        _dragOffset = _mousePosition - (Vector2)transform.position;

        _isDragging = true;
    }

    private void OnMouseUp()
    {
        _isDragging = false; // Stop dragging on release
    }

    public void StartManualDrag(Vector2 mousePos)
    {
        _isDragging = true;
        // Recalculate mouse world position to be absolutely sure the offset is fresh
        Vector2 currentWorldMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _dragOffset = currentWorldMouse - (Vector2)transform.position;
    }

    private void UpdateObjectPosition()
    {
        if (!_isDragging) return;

        // 1. Calculate the raw target position based on the mouse
        float newXPos = _mousePosition.x - _dragOffset.x;
        float newYPos = _mousePosition.y - _dragOffset.y;

        // 2. Clamp the raw position so it cannot exceed your screen boundaries
        newXPos = Mathf.Clamp(newXPos, _minX, _maxX);
        newYPos = Mathf.Clamp(newYPos, _minY, _maxY);

        // 3. Apply the clamped movement while maintaining original Z depth
        transform.position = new Vector3(newXPos, newYPos, transform.position.z);
    }

}