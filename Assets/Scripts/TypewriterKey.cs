using UnityEngine;
using System;

// Script for keys on typewriter GameObjects 
// Each key (26 total) is instantiated from this class 
public class TypewriterKey : MonoBehaviour
{

    // Important Action, to be accessed from TextType.cs, ClickToTranscribe.cs, TypewriterHandMovement.cs
    // Only TypewriterKey can invoke Action
    public static event Action<bool> OnCanTypeChanged;

    // Input Key Code, unique to each initialized key object
    [SerializeField] private KeyCode _keyInput = KeyCode.None;
    private Vector3 _ogPos, _shiftPos;
    private SpriteRenderer _spriteRenderer;
    private Color32 _spriteColor;

    // To check if typewriter keys are activated
    private static bool _canType = false;
    public static bool CanType // Added encapsulation
    {
        get => _canType;
        set
        {
            // If _canType hasn't been changed, return
            if (_canType == value) return;

            // If changed, assign new value
            // Invoke OnCanTypeChanged Action
            _canType = value;
            OnCanTypeChanged?.Invoke(_canType);
        }
    }

    
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null) _spriteColor = _spriteRenderer.color;

        // Key's typed and untyped position
        _ogPos = transform.position;
        _shiftPos = _ogPos + new Vector3(0, -0.03f, 0);
    }

    private void OnEnable()
    {
        // Subscribe to Action
        OnCanTypeChanged += UpdateVisuals;
        UpdateVisuals(_canType); // Pass bool logic into argument
    }

    private void OnDisable() => OnCanTypeChanged -= UpdateVisuals;

    // Take bool passed into perimeter (from Invoke in public CanType)
    private void UpdateVisuals(bool canTypeState)
    {
        if (_spriteRenderer == null) return;

        // Can't do _spriteRenderer.color.a for . . . reasons
        _spriteColor.a = canTypeState ? (byte)255 : (byte)150;
        _spriteRenderer.color = _spriteColor;
    }

    private void Update()
    {
        if (!_canType) return; // Return if not typable
        transform.position = Input.GetKey(_keyInput) ? _shiftPos : _ogPos;
    }
}
