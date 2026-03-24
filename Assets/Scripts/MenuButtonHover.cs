using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{

    [SerializeField] private bool _isMenuButton = false;
    [SerializeField] private GameObject _highlight;

    public void OnPointerEnter(PointerEventData eventData)
    {
        AudiopoolSFX.Instance.Play("SFX_PaperFolds");
        if (_isMenuButton) {
            _highlight.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AudiopoolSFX.Instance.Play("SFX_PaperFolds");
        if (_isMenuButton)
        {
            _highlight.SetActive(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        AudiopoolSFX.Instance.Play("SFX_ButtonPress");
    }


}