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
    private void HandleCanTypeChanged(bool canType)
    {
        _canType = canType;
        if (!canType) _textMesh.text = ""; // If CanType is false, no text appears
    }

    void Update()
    {
        // If CanType (from TypewriterKey.cs) is false, player can not type and engage word check
        if (!_canType || _allWords == null) return;

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

    // 
    private void UpdateVisuals()
    {
        if (_allWords == null || _wordIndex >= _allWords.Length) return;

        if (!_hasMistake)
        {
            _textMesh.text = _currentInput;
        }
        else
        {
            string correctPart = _currentInput.Substring(0, _currentInput.Length - 1);
            char wrongChar = _currentInput[_currentInput.Length - 1];
            _textMesh.text = $"{correctPart}<color=#FFD402>{wrongChar}</color>";
        }
    }

    private void TriggerShake()
    {
        // Safety reset mechanism
        if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine); // If player types another wrong letter WHILE it's shaking, stop current coroutine
        _shakeCoroutine = StartCoroutine(ShakeRoutine()); // Restart Coroutine
    }

    // Shake logic
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
        OnCurrentDocumentFinished?.Invoke(); // Lock the highlight
        _wordIndex = 0;
    }
}
