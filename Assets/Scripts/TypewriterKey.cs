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

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(_keyInput))
        {
            transform.position = _shiftPos;
        }
        else {
            transform.position = _ogPos;
        }
    }
}
