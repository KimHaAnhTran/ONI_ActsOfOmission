using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;

// Script for the text above the typewriter on right side desk, attached to Type Box game object
// Does spellcheck and text shake
public class TextType : MonoBehaviour
{
    // Important void Action for to check if document has finished transcribing
    // Action (no <>) denotes no parameter
    public static event Action OnCurrentDocumentFinished; // New event to lock highlights

    private TextMeshProUGUI _textMesh;
    private bool _canType;

    // For accuracy check
    [Header("Shake Settings")]
    [SerializeField] private float _shakeDuration = 0.15f;
    [SerializeField] private float _shakeAmount = 5f;
    private Vector3 _originalLocalPos;
    private Coroutine _shakeCoroutine; // Lets us know if a Coroutine is happening at that moment

    // Word Logic
    private string[] _allWords;
    private int _wordIndex = 0;
    private string _currentInput = ""; 
    private bool _hasMistake = false;

    
    [Header("Color Settings")]
    [SerializeField] private string _errorColor = "#FFD402"; // Yellow
    [SerializeField] private string _ghostColor = "#666666"; // Gray


    private Coroutine _visualsCoroutine;
    [SerializeField] private float _startDelay = 1f;

    // --- NEW STATS DATA ---
    private float _docStartTime; // When the player actually started typing this doc
    private bool _timerActive = false; // Is the clock running?

    // This data type is Expression Bodied Property in C#, read-only
    // If everything goes right (list not empty, wordIndex within bounds), return word player needs to type
    private string TargetWord => (_allWords != null && _wordIndex < _allWords.Length) ? _allWords[_wordIndex] : "";

    void Awake()
    {
        _textMesh = GetComponent<TextMeshProUGUI>();
        _textMesh.richText = true;
        _originalLocalPos = transform.localPosition;
    }

    // Separate this so it can be called whenever a new document arrives
    public void UpdateDocumentContent()
    {
        string content = MainDataset.GetNextDocumentContent();

        if (content != "End of Records")
        {
            _allWords = content.Split(' ');
            _wordIndex = 0;
            _currentInput = "";
            _hasMistake = false;
            
            // --- RESET DOC TIMER ---
            _timerActive = false; 

            if (_visualsCoroutine != null) StopCoroutine(_visualsCoroutine);
            _visualsCoroutine = StartCoroutine(DelayedStartRoutine());
        }
        else
        {
            _textMesh.text = "NO MORE DOCUMENTS";
        }
    }

    private void OnEnable()
    {
        // Subscribe to OnCanTypeChanged, set up in TypewriterKey.cs
        TypewriterKey.OnCanTypeChanged += HandleCanTypeChanged;
        _canType = TypewriterKey.CanType;
    }

    private void OnDisable()
    {
        TypewriterKey.OnCanTypeChanged -= HandleCanTypeChanged;
        StopAllCoroutines(); // Stop text vibration
        transform.localPosition = _originalLocalPos;
    }

    // Method subscribed to OnCanTypeChanged Action
    // TypewriterKey.cs invokes only, this class can not
    // Add this to your Private Fields
    private void HandleCanTypeChanged(bool canType)
    {
        _canType = canType;

        // Stop any existing routine to prevent overlap
        if (_visualsCoroutine != null) StopCoroutine(_visualsCoroutine);

        if (canType)
        {
            // Update the next word target
            UpdateDocumentContent();

            // Start the delayed reveal
            _visualsCoroutine = StartCoroutine(DelayedStartRoutine());
        }
        else
        {
            _textMesh.text = ""; // Clear immediately if disabled
        }
    }

    private IEnumerator DelayedStartRoutine()
    {
        _textMesh.text = ""; // Ensure it's empty during the wait
        yield return new WaitForSeconds(_startDelay);
        UpdateVisuals();
        _visualsCoroutine = null;
    }

    void Update()
    {
        if (!_canType || _allWords == null || _visualsCoroutine != null) return;

        // Accumulate time only while typing is active and allowed
        if (_timerActive)
        {
            GameManager.TotalTypingTime += Time.deltaTime;
        }

        foreach (char c in Input.inputString)
        {
            // Start timer on first real keypress
            if (!_timerActive && !char.IsControl(c))
            {
                _timerActive = true;
            }

            if (c == '\b') // Backspace
            {
                AudiopoolSFX.Instance.Play("SFX_ButtonPullUp");
                if (_currentInput.Length > 0)
                {
                    _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
                    ValidateInput();
                }
            }
            else if (c == ' ' || c == '\n' || c == '\r') // Word Completion Keys
            {
                // If the word is correct, move to the next word
                if (!_hasMistake && _currentInput == TargetWord)
                {
                    AudiopoolSFX.Instance.Play("SFX_ButtonPullUp");

                    // --- CHARACTER TRACKING ---
                    // We count the 'space' as a character for WPM accuracy
                    GameManager.TotalCharactersTyped++;

                    _wordIndex++;
                    _currentInput = "";

                    if (_wordIndex >= _allWords.Length)
                        FinishDocument();
                }
                else
                {
                    // Trying to space out of a wrong/incomplete word
                    TriggerShake();
                }
            }
            else // Normal Letter Input
            {
                if (!_hasMistake)
                {
                    AudiopoolSFX.Instance.Play("SFX_Typewriter1");
                    _currentInput += c;

                    // --- CHARACTER TRACKING ---
                    GameManager.TotalCharactersTyped++;

                    ValidateInput();
                    if (_hasMistake) TriggerShake();
                }
                else
                {
                    TriggerShake();
                }
            }
        }
        UpdateVisuals();
    }

    // Accuracy check
    private void ValidateInput()
    {
        _hasMistake = !TargetWord.StartsWith(_currentInput);
    }

    private void UpdateVisuals()
    {
        if (_allWords == null || _wordIndex >= _allWords.Length) return;

        

        string target = TargetWord;
        string formattedText = "";

        if (!_hasMistake)
        {
            // 1. Everything typed so far is correct (White)
            // 2. The rest of the target word is ghost text (Gray)
            string typed = _currentInput;
            string untyped = target.Substring(typed.Length);

            formattedText = $"{typed}<color={_ghostColor}>{untyped}</color>";
        }
        else
        {
            // Example: Target "Climb", Input "Cla"
            // correctPart = "Cla" (actually "Cl" is correct, but the logic 
            // follows the input length minus the mistake)

            string correctPart = _currentInput.Substring(0, _currentInput.Length - 1);
            char wrongChar = _currentInput[_currentInput.Length - 1];

            // Calculate how much of the original word is left AFTER the mistake
            // If target is "Climb" (5) and we typed "Cla" (3), we skip "i" and show "mb"
            int remainingStart = _currentInput.Length;
            string remainingGhost = "";

            if (remainingStart < target.Length)
            {
                remainingGhost = target.Substring(remainingStart);
            }

            formattedText = $"{correctPart}<color={_errorColor}>{wrongChar}</color><color={_ghostColor}>{remainingGhost}</color>";
        }

        _textMesh.text = formattedText;
    }

    private void TriggerShake()
    {
        AudiopoolSFX.Instance.Play("SFX_PaperWobbles");

        // --- LOG ERROR ---
        GameManager.TotalErrors++;

        // Safety reset mechanism
        if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine); // If player types another wrong letter WHILE it's shaking, stop current coroutine
        _shakeCoroutine = StartCoroutine(ShakeRoutine()); // Restart Coroutine
    }

    // Shake logic for when player enters wrong char
    private IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;
        while (elapsed < _shakeDuration)
        {
            float xOffset = UnityEngine.Random.Range(-1f, 1f) * _shakeAmount;
            transform.localPosition = new Vector3(_originalLocalPos.x + xOffset, _originalLocalPos.y, _originalLocalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = _originalLocalPos;
    }

    // Document is finish trascribed
    private void FinishDocument()
    {
        _timerActive = false; // Just stop the clock

        TypewriterKey.CanType = false;
        _textMesh.text = "";
        OnCurrentDocumentFinished?.Invoke();
        _wordIndex = 0;
    }
}