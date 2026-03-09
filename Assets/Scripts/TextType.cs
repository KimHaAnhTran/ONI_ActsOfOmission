using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;

public class TextType : MonoBehaviour
{
    public static event Action OnCurrentDocumentFinished; // New event to lock highlights

    private TextMeshProUGUI _textMesh;
    private bool _canType;

    [Header("Shake Settings")]
    [SerializeField] private float _shakeDuration = 0.15f;
    [SerializeField] private float _shakeAmount = 5f;
    private Vector3 _originalLocalPos;
    private Coroutine _shakeCoroutine;

    // Word Logic
    private string[] _allWords;
    private int _wordIndex = 0;
    private string _currentInput = "";
    private bool _hasMistake = false;

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
            _allWords = MainDataset.DocumentGroups[0][0].Split(' ');
            UpdateVisuals();
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
        transform.localPosition = _originalLocalPos;
    }

    private void HandleCanTypeChanged(bool canType)
    {
        _canType = canType;
        if (!canType) _textMesh.text = "";
    }

    void Update()
    {
        if (!_canType || _allWords == null) return;

        foreach (char c in Input.inputString)
        {
            if (c == '\b')
            {
                if (_currentInput.Length > 0)
                {
                    _currentInput = _currentInput.Substring(0, _currentInput.Length - 1);
                    ValidateInput();
                }
            }
            else if (c == ' ' || c == '\n' || c == '\r')
            {
                if (!_hasMistake && _currentInput == TargetWord)
                {
                    _wordIndex++;
                    _currentInput = "";

                    if (_wordIndex >= _allWords.Length)
                        FinishDocument();
                }
                else
                {
                    TriggerShake();
                }
            }
            else
            {
                if (!_hasMistake)
                {
                    _currentInput += c;
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

    private void ValidateInput()
    {
        bool wasMistake = _hasMistake;
        _hasMistake = !TargetWord.StartsWith(_currentInput);

        // Notify the Typewriter Hand to jam or unjam
        if (wasMistake != _hasMistake)
            TypewriterKey.SetMistakeState(_hasMistake);
    }

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
        if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
        _shakeCoroutine = StartCoroutine(ShakeRoutine());
    }

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

    private void FinishDocument()
    {
        TypewriterKey.CanType = false;
        TypewriterKey.SetMistakeState(false); // Unjam hand for next paper
        OnCurrentDocumentFinished?.Invoke(); // Lock the highlight
        _wordIndex = 0;
    }
}