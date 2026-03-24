using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Opening Day UI")]
    [SerializeField] private GameObject _openingTextGroup;

    [Header("End of Day UI")]
    [SerializeField] private GameObject _endTextGroup;
    [SerializeField] private TextMeshProUGUI _rightScoreText;
    [SerializeField] private float _delayBetweenElements = 1.0f;

    // --- STAT TRACKING ---
    public static int TotalErrors { get; set; }
    public static int TotalCharactersTyped { get; set; } // Change this from Words to Characters
    public static float TotalTypingTime { get; set; }

    // Use the standard 5-character-per-word formula
    public static float CurrentWPM => TotalTypingTime > 0
        ? ((TotalCharactersTyped / 5f) / (TotalTypingTime / 60f))
        : 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Ensure UI is hidden at start
        if (_endTextGroup != null) _endTextGroup.SetActive(false);
    }

    private void Start()
    {
        _endTextGroup.SetActive(false);
        _openingTextGroup.SetActive(true);

        MainDataset.CheckDay();
        // Reset stats at the start of every new day/scene
        TotalErrors = 0;
        TotalTypingTime = 0;
    }

    // Called from VoicemailClick.cs
    public void TriggerDayDialogue()
    {
        // 1. Get the current Day Index from MainDataset (Day 1 = 0, Day 2 = 1, etc.)
        int currentDayIndex = MainDataset.GetGroupIndex();

        // 2. Convert index back to a "Day#" string to match your file naming
        // Example: Day 1 index is 0 -> FileName "Day1_Intro"
        string dialogueFileName = $"Day{currentDayIndex + 1}_Intro";

        // 3. Tell the Dialogue Controller to play it
        if (PalDialogueController.Instance != null)
        {
            Debug.Log($"GameManager: Triggering dialogue for {dialogueFileName}");
            PalDialogueController.Instance.TriggerDialogue(dialogueFileName);
        }
        else
        {
            Debug.LogError("GameManager: PalDialogueController instance not found!");
        }
    }

    public void LoadNextDay(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void OnStartDayButtonClicked()
    {
        Fade fader = GameObject.FindWithTag("Fade").GetComponent<Fade>();

        if (fader != null)
        {
            fader.StartFadeIn();
            Debug.Log("GameManager: Start Day triggered, fading in.");
        }

        GameObject startDayUI = GameObject.FindWithTag("StartDayUI");
        if (startDayUI != null)
        {
            startDayUI.SetActive(false);
        }
    }

    // Handle the transition logic
    public void StartEndOfDayTransition()
    {
        StartCoroutine(EndOfDaySequence());
    }

    private IEnumerator EndOfDaySequence()
    {
        Fade fader = GameObject.FindWithTag("Fade").GetComponent<Fade>();

        if (fader != null)
        {
            fader.StartFadeOut();
            yield return new WaitForSeconds(3.0f); // Wait for fade to settle
        }

        // --- TRIGGER RESULTS SCREEN ---
        if (_endTextGroup != null)
        {
            yield return StartCoroutine(RevealResultsRoutine());
        }
    }

    private IEnumerator RevealResultsRoutine()
    {
        // 1. Prepare the numbers
        _rightScoreText.text = $"{Mathf.RoundToInt(CurrentWPM)}\n{TotalErrors}";

        // 2. Hide all children initially
        foreach (Transform child in _endTextGroup.transform)
        {
            child.gameObject.SetActive(false);
        }

        // 3. Enable the parent group
        _endTextGroup.SetActive(true);

        // 4. Reveal children one by one with a delay
        foreach (Transform child in _endTextGroup.transform)
        {
            child.gameObject.SetActive(true);
            
            // Play a small "blip" sound
            AudiopoolSFX.Instance.Play("SFX_PaperFolds"); 

            yield return new WaitForSeconds(_delayBetweenElements);
        }
    }

    // Method for your "Next Chapter" button to call
    public void LoadNextScene()
    {
        StartCoroutine(LoadSceneRoutine());
    }

    private IEnumerator LoadSceneRoutine()
    {
        // 1. Play the sound
        AudiopoolSFX.Instance.Play("SFX_ButtonPress");

        yield return new WaitForSeconds(0.5f);

        // 4. Now switch scenes safely
        int currentDay = MainDataset.GetGroupIndex() + 1;
        string nextDayScene = "Day" + (currentDay + 1);
        SceneManager.LoadScene(nextDayScene);

    }

    private void OnEnable()
    {
        // Call "OnSceneLoaded" every time a scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Always unsubscribe when the object is disabled to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Since GameManager.cs is not destroyed each scene, this must be the case
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. FORCE the day check immediately
        MainDataset.CheckDay();
        int currentDayIndex = MainDataset.GetGroupIndex();
        Debug.Log($"GameManager: Scene {scene.name} loaded. Calculated Day Index: {currentDayIndex}");

        // 2. Reset stats
        TotalErrors = 0;
        TotalCharactersTyped = 0;
        TotalTypingTime = 0;

        // 3. UI Setup
        if (_endTextGroup != null) _endTextGroup.SetActive(false);

        // We set this to true AFTER we update the text
        if (_openingTextGroup != null) _openingTextGroup.SetActive(true);

        // 4. Update the UI
        if (ChapterNameUpdate.Instance != null)
        {
            Debug.Log("Current Day Index: " + currentDayIndex);
            ChapterNameUpdate.Instance.UpdateChapterUI(currentDayIndex);
        }
        else
        {
            Debug.Log("Child null");
            // If Instance is null, find it manually (backup for first scene load)
            GetComponentInChildren<ChapterNameUpdate>().UpdateChapterUI(currentDayIndex);
        }

        // 5. Trigger Typewriter
        TypewriterSequence seq = _openingTextGroup.GetComponent<TypewriterSequence>();
        if (seq != null)
        {
            seq.StartSequence();
        }
    }

}