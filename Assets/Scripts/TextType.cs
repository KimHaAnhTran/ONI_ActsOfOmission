using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextType : MonoBehaviour
{
    private TextMeshProUGUI _textMesh;
    private bool _canType;

    void Awake()
    {
        _textMesh = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        // Subscribe to the event for better performance
        TypewriterKey.OnCanTypeChanged += HandleCanTypeChanged;
        _canType = TypewriterKey.CanType;
    }

    private void OnDisable()
    {
        TypewriterKey.OnCanTypeChanged -= HandleCanTypeChanged;
    }

    private void HandleCanTypeChanged(bool canType)
    {
        _canType = canType;
    }

    void Update()
    {
        if (!_canType) return;

        // inputString captures everything typed this frame
        foreach (char c in Input.inputString)
        {
            if (c == '\b') // Backspace
            {
                if (_textMesh.text.Length > 0)
                {
                    _textMesh.text = _textMesh.text.Substring(0, _textMesh.text.Length - 1);
                }
            }
            else if (c == '\n' || c == '\r' || c == ' ') // Enter / Return
            {
                // RESET the text field
                _textMesh.text = "";
            }
            else // Any other character
            {
                _textMesh.text += c;
            }
        }
    }
}