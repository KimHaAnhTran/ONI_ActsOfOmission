using UnityEngine;
using TMPro;
using System.Collections;
using System;

public class TypeStartMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI _targetTextMesh; // Drag-dropped in Inspector

    [Header("Settings")]
    [SerializeField] private string _targetWord = "START";
    [SerializeField] private float _startDelay = 0.5f; // New delay setting
    [SerializeField] private string _ghostColor = "#666666";
    [SerializeField] private string _errorColor = "#FFD402";

    [Header("Shake Settings")]
    [SerializeField] private float _shakeDuration = 0.15f;
    [SerializeField] private float _shakeAmount = 5f;

    private string _currentInput = "";
    private bool _hasMistake = false;
    private Vector3 _originalPos;
    private Coroutine _shakeCoroutine;
    private bool _isActive = false;
    private bool _canType = false;

    private void Awake()
    {
        if (_targetTextMesh != null)
        {
            _originalPos = _targetTextMesh.transform.localPosition;
            _targetTextMesh.text = ""; // Ensure it's empty on boot
        }
    }

    private void OnEnable()
    {
        TypewriterKey.OnCanTypeChanged += HandleCanTypeChanged;
        _canType = TypewriterKey.CanType;
    }

    private void OnDisable()
    {
        TypewriterKey.OnCanTypeChanged -= HandleCanTypeChanged;
        StopAllCoroutines();
    }

    private void HandleCanTypeChanged(bool canType)
    {
        _canType = canType;

        // --- MODIFIED LOGIC ---
        // Only clear the text if we ARE active (still typing) but the typewriter was forced off.
        // If _isActive is false, it means we finished the word, so we want to keep the text visible.
        if (!canType && _isActive && _targetTextMesh != null)
        {
            _targetTextMesh.text = "";
        }
        else if (canType && _isActive)
        {
            UpdateVisuals();
        }
    }

    public void ActivateTyping()
    {
        _currentInput = "";
        _hasMistake = false;
        _isActive = true;

        // Start the routine that handles the technical fix and your 0.5s delay
        StartCoroutine(EnableTypewriterRoutine());
    }

    private IEnumerator EnableTypewriterRoutine()
    {
        // 1. Technical Fix: Wait 1 frame so TypewriterKeys can finish OnEnable subscriptions
        yield return null;

        // 2. Visual Polish: Wait the requested 0.5s before anything appears
        yield return new WaitForSeconds(_startDelay);

        // 3. Turn on the keys and show the "START" ghost text
        TypewriterKey.CanType = true;
        UpdateVisuals();

        Debug.Log("<color=green>TypeStartMenu:</color> Sequence Complete. CanType is TRUE.");
    }

    private void Update()
    {
        // Guard clause: Don't process input if we aren't active or keys are disabled
        if (!_isActive || !_canType || _targetTextMesh == null) return;

        foreach (char c in Input.inputString)
        {
            if (c == '\b') // Backspace (Always allow this)
            {
                if (_currentInput.Length > 0)
                {
                    _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
                    AudiopoolSFX.Instance.Play("SFX_Typewriter1");

                    // After backspacing, check if we've cleared the mistake
                    _hasMistake = !_targetWord.StartsWith(_currentInput);
                }
            }
            else if (c == '\n' || c == '\r' || c == ' ') // Enter
            {
                if (!_hasMistake && _currentInput == _targetWord)
                {
                    FinishStart();
                }
                else
                {
                    TriggerShake();
                }
            }
            else // Normal typing
            {
                // --- LOCK LOGIC ---
                // If there's already a mistake, any new letter keypress just shakes.
                if (_hasMistake)
                {
                    TriggerShake();
                }
                else if (_currentInput.Length < _targetWord.Length)
                {
                    // If no mistake yet, try to add the new letter
                    string nextInput = _currentInput + char.ToUpper(c);

                    if (_targetWord.StartsWith(nextInput))
                    {
                        // Correct letter
                        _currentInput = nextInput;
                        AudiopoolSFX.Instance.Play("SFX_Typewriter1");
                    }
                    else
                    {
                        // Wrong letter - Set mistake to true and shake
                        _currentInput = nextInput; // We still record it to show the yellow error char
                        _hasMistake = true;
                        TriggerShake();
                    }
                }
                else
                {
                    // Typing beyond word length
                    TriggerShake();
                }
            }
            UpdateVisuals();
        }
    }

    private void UpdateVisuals()
    {
        // Removed the !_canType guard here so that the final "START" 
        // remains rendered even after FinishStart() sets CanType to false.
        if (_targetTextMesh == null) return;

        if (!_hasMistake)
        {
            string typed = _currentInput;
            string untyped = _targetWord.Substring(typed.Length);
            _targetTextMesh.text = $"{typed}<color={_ghostColor}>{untyped}</color>";
        }
        else
        {
            // Hard-lock logic: only shows error if we haven't backspaced it yet
            string correctPart = _currentInput.Substring(0, Mathf.Max(0, _currentInput.Length - 1));
            char wrongChar = _currentInput[_currentInput.Length - 1];

            int ghostStart = Mathf.Min(_currentInput.Length, _targetWord.Length);
            string remainingGhost = _targetWord.Substring(ghostStart);

            _targetTextMesh.text = $"{correctPart}<color={_errorColor}>{wrongChar}</color><color={_ghostColor}>{remainingGhost}</color>";
        }
    }

    private void TriggerShake()
    {
        AudiopoolSFX.Instance.Play("SFX_PaperWobbles");
        if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
        _shakeCoroutine = StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;
        while (elapsed < _shakeDuration)
        {
            float xOffset = UnityEngine.Random.Range(-1f, 1f) * _shakeAmount;
            _targetTextMesh.transform.localPosition = _originalPos + new Vector3(xOffset, 0, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }
        _targetTextMesh.transform.localPosition = _originalPos;
    }

    private void FinishStart()
    {
        AudiopoolSFX.Instance.Play("SFX_ButtonPullUp");
        _isActive = false;
        TypewriterKey.CanType = false; // Turn keys off for the transition
        MenuManager.Instance.BeginGameTransition();
    }
}