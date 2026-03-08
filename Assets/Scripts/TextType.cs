using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextType : MonoBehaviour
{
    private TextMeshProUGUI _textMesh;

    void Start()
    {
        // Get the component at the start
        _textMesh = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        // inputString captures everything typed this frame
        foreach (char c in Input.inputString)
        {
            if (c == '\b') // If backspace is pressed
            {
                if (_textMesh.text.Length > 0)
                {
                    _textMesh.text = _textMesh.text.Substring(0, _textMesh.text.Length - 1);
                }
            }
            else if ((c == '\n') || (c == '\r')) // If enter/return is pressed
            {
                _textMesh.text += "\n";
            }
            else // Any other character typed
            {
                _textMesh.text += c;
            }
        }
    }
}
