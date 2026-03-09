using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypewriterHandMovement : MonoBehaviour
{
    private List<float> _XPos;
    private int _positionIndex = 0;
    private bool _canType;

    [Header("Smoothness Settings")]
    [SerializeField] private float _smoothTime = 0.15f; // Adjust for speed of the slide
    private float _targetX;
    private float _currentXVelocity = 0f;

    private void Awake()
    {
        _XPos = new List<float> { 0.05f, 0.019f, -0.003f, -0.023f, -0.057f };

        // Initialize target so it doesn't fly in from (0,0,0)
        _targetX = _XPos[2];
        Vector3 pos = transform.localPosition;
        pos.x = _targetX;
        transform.localPosition = pos;
    }

    private void OnEnable()
    {
        TypewriterKey.OnCanTypeChanged += HandleTypeStateChanged;
        HandleTypeStateChanged(TypewriterKey.CanType);
    }

    private void OnDisable()
    {
        TypewriterKey.OnCanTypeChanged -= HandleTypeStateChanged;
    }

    private void HandleTypeStateChanged(bool canType)
    {
        _canType = canType;
        if (!canType) ResetPosition();
    }

    private void ResetPosition()
    {
        _positionIndex = 2;
        _targetX = _XPos[2];
    }

    private void Update()
    {
        // 1. Always calculate the smooth movement toward the targetX
        // This runs even if _canType is false so the reset slide is smooth too!
        float currentX = transform.localPosition.y; // Wait, let's fix the axis logic



        Vector3 localPos = transform.localPosition;
        float actualSmoothTime = (_positionIndex == 0) ? _smoothTime * 2f : _smoothTime;

        localPos.x = Mathf.SmoothDamp(localPos.x, _targetX, ref _currentXVelocity, actualSmoothTime);
        transform.localPosition = localPos;

        // 2. Only allow the user to change the target if typing is active
        if (!_canType) return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            _targetX = _XPos[_positionIndex];
            _positionIndex = (_positionIndex + 1) % _XPos.Count;
        }
    }
}