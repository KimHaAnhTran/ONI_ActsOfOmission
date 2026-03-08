using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TextHover : MonoBehaviour
{
    [SerializeField] private GameObject _highlights;
    private void Start()
    {
        _highlights.SetActive(false);
    
    }

    public void OnMouseOver()
    {
        _highlights.SetActive(true);
    }

    public void OnMouseExit()
    {
        _highlights.SetActive(false);
    }

}
