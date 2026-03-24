using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Required for Scene switching

public class EndManager : MonoBehaviour
{
    [Header("UI Swap Settings")]
    [SerializeField] private SpriteRenderer _targetImage;
    [SerializeField] private Sprite _newSprite;
    [SerializeField] private GameObject _buttonToHide;
    [SerializeField] private GameObject _buttonToShow;

    [SerializeField] private GameObject _textToHide;

    [Header("Group Toggle Settings")]
    [SerializeField] private List<GameObject> _groupToDisable;
    [SerializeField] private List<GameObject> _groupToEnable;

    [Header("Reset Settings")]
    [SerializeField] private string _mainMenuSceneName = "0_Menu";

    public void OnFirstButtonClick()
    {
        if (_targetImage != null && _newSprite != null) _targetImage.sprite = _newSprite;
        if (_buttonToHide != null) _buttonToHide.SetActive(false);
        if (_textToHide != null) _textToHide.SetActive(false);
        if (_buttonToShow != null) _buttonToShow.SetActive(true);
    }

    public void OnSecondButtonClick()
    {
        foreach (GameObject obj in _groupToDisable) if (obj != null) obj.SetActive(false);
        foreach (GameObject obj in _groupToEnable) if (obj != null) obj.SetActive(true);
    }

    // Complete Game Reset: Wipes persistent objects and reloads the menu
    public void OnThirdButtonClick()
    {

        // 2. Reset the static typewriter state so the menu doesn't start "On"
        TypewriterKey.CanType = false;

        // 3. Load the first scene
        SceneManager.LoadScene(_mainMenuSceneName);
    }

}