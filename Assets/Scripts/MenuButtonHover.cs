using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        AudiopoolSFX.Instance.Play("SFX_PaperFolds");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AudiopoolSFX.Instance.Play("SFX_PaperFolds");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        AudiopoolSFX.Instance.Play("SFX_ButtonPress");
    }

}