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

    [Header("Cleanup Settings")]
    [SerializeField] private string _endSceneName = "Z_Ending"; // Change this to your actual final scene name

    // --- STAT TRACKING ---
    public static int TotalErrors { get; set; }
    public static int TotalCharactersTyped { get; set; } 
    public static float TotalTypingTime { get; set; }
    
    // Track how many documents timed out
    public static int TotalLateDocuments { get; set; }

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
        // Reset stats
        TotalErrors = 0;
        TotalCharactersTyped = 0;
        TotalTypingTime = 0;

        // Reset late documents for the new day
        TotalLateDocuments = 0;
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
        int totalDocsToday = MainDataset.GetTotalDocumentsForCurrentDay();
        _rightScoreText.text = $"{Mathf.RoundToInt(CurrentWPM)}\n{TotalErrors}\n{TotalLateDocuments}/{totalDocsToday}";

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
        yield return new WaitForSeconds(0.5f);

        // Get the day number we just finished (Day 1 = 1, Day 4 = 4)
        int finishedDay = MainDataset.GetGroupIndex() + 1;

        // CHECK: If we just finished the last day, go to the End Scene
        if (finishedDay >= 4)
        {
            Fade fader = GameObject.FindWithTag("Fade2").GetComponent<Fade>();

            if (fader != null)
            {
                fader.StartFadeOut();
                yield return new WaitForSeconds(4.0f); // Wait for fade 
            }

            Debug.Log("GameManager: All days complete. Loading End Scene.");
            SceneManager.LoadScene(_endSceneName);
            Destroy(gameObject);
        }
        else
        {
            // Otherwise, proceed to the next day
            string nextDayScene = "Day" + (finishedDay + 1);
            Debug.Log($"GameManager: Loading {nextDayScene}");
            SceneManager.LoadScene(nextDayScene);
        }
    }

    private void OnEnable()
    {
        // Call "OnSceneLoaded" every time a scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
        TimerManager.OnTimerRanOut += HandleLateDocument;
    }

    private void OnDisable()
    {
        // Always unsubscribe when the object is disabled to prevent memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
        TimerManager.OnTimerRanOut -= HandleLateDocument;
    }

    // Increment the late tracker when the signal fires ---
    private void HandleLateDocument()
    {
        TotalLateDocuments++;
    }

    // Since GameManager.cs is not destroyed each scene, this must be the case
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        // --- NEW CLEANUP CHECK ---
        // If we have arrived at the final scene, this manager is no longer needed.
        if (scene.name == _endSceneName)
        {
            Debug.Log("GameManager: Final scene reached. Destroying GameManager.");
            Instance = null; // Clear the static instance so it doesn't point to a dead object
            Destroy(this);
            return; // Exit the method so we don't run the rest of the setup logic
        }

        // 1. FORCE the day check immediately
        MainDataset.CheckDay();
        int currentDayIndex = MainDataset.GetGroupIndex();
        Debug.Log($"GameManager: Scene {scene.name} loaded. Calculated Day Index: {currentDayIndex}");

        // 2. Reset stats
        TotalErrors = 0;
        TotalCharactersTyped = 0;
        TotalTypingTime = 0;

        // Reset late documents for the new day
        TotalLateDocuments = 0;

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