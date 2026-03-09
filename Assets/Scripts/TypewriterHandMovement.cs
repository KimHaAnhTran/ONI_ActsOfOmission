using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypewriterHandMovement : MonoBehaviour
{
    private List<float> _XPos;
    private int _positionIndex = 0;

    // We don't even need a local _canType bool if we use the event correctly, 
    // but keeping a private one helps keep Update() clean.
    private bool _canType;

    private void Awake()
    {
        _XPos = new List<float> { 0.05f, 0.019f, -0.003f, -0.023f, -0.057f };
        ResetPosition();
    }

    private void OnEnable()
    {
        // Subscribe to the same event the keys use!
        TypewriterKey.OnCanTypeChanged += HandleTypeStateChanged;

        // Sync initial state
        HandleTypeStateChanged(TypewriterKey.CanType);
    }

    private void OnDisable()
    {
        // Always unsubscribe
        TypewriterKey.OnCanTypeChanged -= HandleTypeStateChanged;
    }

    private void HandleTypeStateChanged(bool canType)
    {
        _canType = canType;

        if (!canType)
        {
            ResetPosition();
        }
    }

    private void ResetPosition()
    {
        _positionIndex = 2; //Original, middle position
        UpdateTransformX(_XPos[2]);
    }

    private void Update()
    {
        if (!_canType) return;

        //Typewriter handle shifts whenever "Space" or "Enter" is pressed
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            UpdateTransformX(_XPos[_positionIndex]);
            _positionIndex = (_positionIndex + 1) % _XPos.Count; // Cleaner way to loop index
        }
    }

    // Helper method to avoid writing "new Vector3" over and over
    private void UpdateTransformX(float x)
    {
        Vector3 pos = transform.localPosition;
        pos.x = x;
        transform.localPosition = pos;
    }
}