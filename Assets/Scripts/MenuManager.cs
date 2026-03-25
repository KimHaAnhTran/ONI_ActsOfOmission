using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("Menu Elements")]
    [SerializeField] private GameObject _startButton;
    [SerializeField] private GameObject _startPaperPrefab;
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private string _firstSceneName = "Day1";
    [SerializeField] private Transform _menuPaperTarget; 
    [SerializeField] private TMP_Text _textMeshPro;

    private void Awake()
    {
        // Simple instance for the Menu Scene
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Called by the UI Button "Start"
    public void OnStartButtonClicked()
    {
        _startButton.SetActive(false);
        _textMeshPro.text = "TYPE '<color=#FFF540>START</color>' THEN ENTER";
        SpawnStartPaper();
    }

    private void SpawnStartPaper()
    {
        if (_startPaperPrefab != null)
        {
            // 1. Instantiate the paper
            GameObject paper = Instantiate(_startPaperPrefab, _spawnPoint.position, _spawnPoint.rotation);
            AudiopoolSFX.Instance.Play("SFX_PaperSlide");

            // 2. Remove the unwanted script from this specific instance
            PaperFadeExit fadeExit = paper.GetComponent<PaperFadeExit>();
            if (fadeExit != null)
            {
                Destroy(fadeExit);
            }

            // 3. Set the new bounce target
            PaperBounceEntry bounce = paper.GetComponent<PaperBounceEntry>();
            if (bounce != null && _menuPaperTarget != null)
            {
                bounce.TargetLocal = _menuPaperTarget;
            }

            // 4. Wake up the typing logic
            TypeStartMenu typingScript = FindObjectOfType<TypeStartMenu>();
            if (typingScript != null)
            {
                typingScript.ActivateTyping();
            }
        }
    }

    // Called by TypeStartMenu once "START" is finished
    public void BeginGameTransition()
    {
        StartCoroutine(MenuToGameRoutine());
    }

    private IEnumerator MenuToGameRoutine()
    {
        Fade fader = GameObject.FindWithTag("Fade").GetComponent<Fade>();

        if (fader != null)
        {
            fader.StartFadeOut();
            // Match your Fade.cs wait times (delay + duration)
            yield return new WaitForSeconds(4f);
        }

        SceneManager.LoadScene(_firstSceneName);
    }
}