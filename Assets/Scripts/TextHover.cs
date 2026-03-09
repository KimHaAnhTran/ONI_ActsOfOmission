using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TextHover : MonoBehaviour
{
    [SerializeField] private GameObject _highlights;
    private  bool _isHover = false;
    private bool _isPressed = false;

    public  bool IsHover {
        get { return _isHover; }
        set { _isHover = value; }
    }
    private void Start()
    {
        _highlights.SetActive(false);
    
    }

    public void OnMouseOver()
    {
        _highlights.SetActive(true);
        _isHover = true;
        _isPressed = TypewriterKey.CanType;
    }

    public void OnMouseExit()
    {
        if (_isPressed) return;
        _highlights.SetActive(false);
        _isHover = false;
    }

}

