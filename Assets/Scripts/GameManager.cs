using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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
    }

    private void Start()
    {
        MainDataset.CheckDay();
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
        // 1. Find the fader in the current scene
        Fade fader = GameObject.FindWithTag("Fade").GetComponent<Fade>();

        if (fader != null)
        {
            // 2. Start fading to Black (Clear to Black)
            fader.StartFadeOut();

            // 3. Wait for the Fade delay + Fade duration (total transition time)
            // We wait slightly longer (0.5s) to let the player sit in the dark for a moment
            yield return new WaitForSeconds(3.5f);
        }

        // 4. Calculate next scene name (e.g., "Day1" -> "Day2")
        int currentDay = MainDataset.GetGroupIndex() + 1;
        string nextDayScene = "Day" + (currentDay + 1);

        // 5. Load the next scene
        SceneManager.LoadScene(nextDayScene);
    }
}