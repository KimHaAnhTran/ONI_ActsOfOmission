using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypewriterKey : MonoBehaviour
{
    [SerializeField] private KeyCode _keyInput = KeyCode.None;
    private Vector3 _ogPos, _shiftPos;

    private void Start()
    {
        _ogPos = transform.position;
        _shiftPos = new Vector3(_ogPos.x, _ogPos.y - .03f, _ogPos.z);
    }

    void Update()
    {
        // Move key down while held
        if (Input.GetKey(_keyInput))
        {
            transform.position = _shiftPos;
        }
        else
        {
            // Return to original position when released
            transform.position = _ogPos;
        }
    }
}