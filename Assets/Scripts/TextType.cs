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

    // This data type is Expression Bodied Property in C#, read-only
    // If everything goes right (list not empty, wordIndex within bounds), return word player needs to type
    private string TargetWord => (_allWords != null && _wordIndex < _allWords.Length) ? _allWords[_wordIndex] : "";

    void Awake()
    {
        _textMesh = GetComponent<TextMeshProUGUI>();
        _textMesh.richText = true;
        _originalLocalPos = transform.localPosition;
    }

    private void Start()
    {
        // For testing: grabs Group 0, Doc 0
        if (MainDataset.DocumentGroups.Count > 0)
        {
            // DocumentGroups[][] is a string
            // Split string via ' '
            _allWords = MainDataset.DocumentGroups[0][0].Split(' ');
            UpdateVisuals();
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
        // Don't process typing or visuals if we can't type 
        // OR if we are still in the initial delay period
        if (!_canType || _allWords == null || _visualsCoroutine != null) return;

        // Check each character in inputted string
        // Input.inputString captures every key pressed since last frame
        // It's checking every frame for any new 'c'
        foreach (char c in Input.inputString) 
        {
            if (c == '\b') // Backspace
            {
                if (_currentInput.Length > 0) // In case player can not backspace into less than 0
                {
                    _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
                    ValidateInput();
                }
            }
            else if (c == ' ' || c == '\n' || c == '\r') // New Line or Space
            {
                // If player's input matches TargetWord
                // allow player to move onto next word in string[]
                if (!_hasMistake && _currentInput == TargetWord)
                {
                    _wordIndex++;
                    _currentInput = "";

                    if (_wordIndex >= _allWords.Length)
                        FinishDocument();
                }
                
                // If player New Line or Space at wrong time
                // Trigger shake wrong
                else
                {
                    TriggerShake();
                }
            }

            // Normal text input and check
            else
            {
                // 1. Are we in a good state?
                if (!_hasMistake)
                {
                    _currentInput += c; // 2. Add new letter player just pressed
                    ValidateInput(); // 3. Check if this letter is right
                    if (_hasMistake) TriggerShake(); //4. If no, shake it. This is the Input Lock
                }
                else
                {
                    // 5. Already in a bad state before this key is pressed
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
        TypewriterKey.CanType = false;
        _textMesh.text = ""; // Clear the ghost text
        OnCurrentDocumentFinished?.Invoke();
        _wordIndex = 0;
    }
}
