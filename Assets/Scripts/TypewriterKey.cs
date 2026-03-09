using UnityEngine;
using System;

public class TypewriterKey : MonoBehaviour
{
    public static event Action<bool> OnCanTypeChanged;

    [SerializeField] private KeyCode _keyInput = KeyCode.None;
    private Vector3 _ogPos, _shiftPos;
    private SpriteRenderer _spriteRenderer;
    private Color32 _spriteColor;

    private static bool _canType = false;
    public static bool CanType
    {
        get => _canType;
        set
        {
            if (_canType == value) return;
            _canType = value;
            OnCanTypeChanged?.Invoke(_canType);
        }
    }


    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _ogPos = transform.position;
        _shiftPos = _ogPos + new Vector3(0, -0.03f, 0);
        if (_spriteRenderer != null) _spriteColor = _spriteRenderer.color;
    }

    private void OnEnable()
    {
        OnCanTypeChanged += UpdateVisuals;
        UpdateVisuals(_canType);
    }

    private void OnDisable() => OnCanTypeChanged -= UpdateVisuals;

    private void UpdateVisuals(bool canTypeState)
    {
        if (_spriteRenderer == null) return;
        _spriteColor.a = canTypeState ? (byte)255 : (byte)150;
        _spriteRenderer.color = _spriteColor;
    }

    private void Update()
    {
        if (!_canType) return;
        transform.position = Input.GetKey(_keyInput) ? _shiftPos : _ogPos;
    }
}