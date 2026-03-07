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

    // Update is called once per frame
    void Update()
    {
        _mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        UpdateObjectPosition();
    }

    //Detect when mouse holds down on object
    private void OnMouseDown()
    {
        //Asset must be offset from mouse a certain distance
        //to give illusion of picking up, instead of teleporting to middle of asset
        _mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _dragOffset = _mousePosition - (Vector2)transform.position;

        _isDragging = true;
    }

    //Detect when mouse does not hold
    private void OnMouseUp()
    {
        _isDragging = false;
    }

    private void UpdateObjectPosition() {
        if (!_isDragging) return;

        float newXPos = _mousePosition.x - _dragOffset.x;
        float newYPos = _mousePosition.y - _dragOffset.y;
        transform.position = new Vector3(newXPos, newYPos, transform.position.z);
    }
}
