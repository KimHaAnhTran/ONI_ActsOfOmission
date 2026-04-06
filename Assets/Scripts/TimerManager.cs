using UnityEngine;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    // Signal that fires when time hits 0
    public static event Action OnTimerRanOut;

    public bool IsTimedOut { get; private set; } = false;

    private GameObject _timerObject;
    private TextMeshProUGUI _timerText;
    private float _timeRemaining;
    private bool _isRunning = false;

    [Header("Timer Settings")]
    [SerializeField] private float _gracePeriodSeconds = 10f;

    // --- NEW: Variables for the flicker effect ---
    private Color _originalColor = Color.white;
    private Color _warningColor;

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
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshTimerReference();
        IsTimedOut = false;
        _isRunning = false;
    }

    // A bulletproof method to find the timer, even if it's currently turned off ---
    private void RefreshTimerReference()
    {
        // FindObjectsOfTypeAll finds objects even if they are currently inactive in the hierarchy
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.CompareTag("Timer") && obj.scene.isLoaded) // Ensure it's in the active scene
            {
                _timerObject = obj;
                _timerText = _timerObject.GetComponent<TextMeshProUGUI>();

                // Store original color and parse the Yellow hex code
                _originalColor = _timerText.color;
                ColorUtility.TryParseHtmlString("#FFF540", out _warningColor);

                _timerObject.SetActive(false);
                break;
            }
        }
    }

    public void StartTimer(int documentWordCount)
    {
        IsTimedOut = false;

        // Failsafe: If the reference was lost, grab it again before starting
        if (_timerObject == null) RefreshTimerReference();

        // 1. Get the player's actual WPM
        float currentWPM = GameManager.CurrentWPM;

        // 2. Custom Calculator: If no WPM is calculated yet, default to 20 seconds.
        if (currentWPM <= 0)
        {
            _timeRemaining = 20f;
        }
        else
        {
            // Time (seconds) = (Words / WPM) * 60. 
            _timeRemaining = ((float)documentWordCount / currentWPM) * 60f + _gracePeriodSeconds;
        }

        // 3. Show the UI and start counting
        if (_timerObject != null)
        {
            _timerObject.SetActive(true);
            if (_timerText != null) _timerText.color = _originalColor; // Reset color to white
        }
        _isRunning = true;
    }

    public void StopTimer()
    {
        _isRunning = false;
        if (_timerObject != null)
        {
            _timerObject.SetActive(false);
            // Ensure color resets if player beats the clock during a flicker
            if (_timerText != null) _timerText.color = _originalColor;
        }
    }

    private void Update()
    {
        if (!_isRunning) return;

        // Failsafe: Force the timer to stay visible while running
        if (_timerObject != null && !_timerObject.activeSelf)
        {
            _timerObject.SetActive(true);
        }

        _timeRemaining -= Time.deltaTime;

        if (_timerText != null)
        {
            // Convert raw seconds into M:SS format
            // CeilToInt ensures 0.1 seconds still shows as 1 second left
            int totalSeconds = Mathf.Max(0, Mathf.CeilToInt(_timeRemaining));
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            // Format string: {0} is minutes, {1:00} forces seconds to always have two digits (e.g., 05)
            _timerText.text = string.Format("{0}:{1:00}", minutes, seconds);

            // --- NEW: Flicker effect for the last 10 seconds ---
            if (_timeRemaining <= 10f && _timeRemaining > 0f)
            {
                // Time.time % 0.5f creates a repeating 0.5-second loop. 
                // For the first 0.25s it's yellow, for the next 0.25s it's white!
                _timerText.color = (Time.time % 0.5f < 0.25f) ? _warningColor : _originalColor;
            }
            else
            {
                // Ensure it stays the normal color if above 10 seconds
                _timerText.color = _originalColor;
            }
        }

        // Time's up!
        if (_timeRemaining <= 0)
        {
            _isRunning = false;
            IsTimedOut = true;

            if (_timerObject != null)
            {
                _timerObject.SetActive(false);
                // Reset the color back to normal for the next document
                if (_timerText != null) _timerText.color = _originalColor;
            }

            // Fire the timeout signal to the rest of the game
            OnTimerRanOut?.Invoke();
        }
    }
}