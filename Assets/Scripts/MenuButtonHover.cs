using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Image _buttonImage;

    // Hex #FFD402 converted to 0-1 range
    private Color _yellowColor = new Color(1f, 0.831f, 0.008f, 1f);
    private Color _blackColor = Color.black;

    private void Awake()
    {
        _buttonImage = GetComponent<Image>();
        if (_buttonImage == null) Debug.LogError("No Image component found on " + gameObject.name);
        _buttonImage.color = _blackColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Mouse Entered Button");
        _buttonImage.color = _yellowColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Mouse Left Button");
        _buttonImage.color = _blackColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Mouse Clicked/Held");
        _buttonImage.color = _yellowColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // If we release while still over the button, stay yellow (hover state)
        if (eventData.pointerCurrentRaycast.gameObject == gameObject)
        {
            _buttonImage.color = _yellowColor;
        }
        else
        {
            _buttonImage.color = _blackColor;
        }
    }
}