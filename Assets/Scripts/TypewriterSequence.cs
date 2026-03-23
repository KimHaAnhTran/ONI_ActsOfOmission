using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))] // Ensures there's a speaker for the continuous SFX
public class TypewriterSequence : MonoBehaviour
{
    [System.Serializable]
    public class TypewriterStep
    {
        public TextMeshProUGUI textElement;
        public float delayAfterFinished = 1.0f;
    }

    [Header("Sequence Order")]
    [SerializeField] private List<TypewriterStep> _sequence = new List<TypewriterStep>();

    [Header("General Settings")]
    [SerializeField] private float _charsPerSecond = 40f;
    [SerializeField] private string _typingSfxName = "SFX_TypewriterContinuous"; // Name in AudiopoolSFX

    [Header("Final Action")]
    [SerializeField] private GameObject _actionButton;
    [SerializeField] private float _delayBeforeButton = 0.5f;

    private AudioSource _audioSource; // Internal reference to the speaker

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>(); // Grab the local AudioSource
        _audioSource.loop = true; // Make sure it loops so the sound doesn't cut out early

        foreach (var step in _sequence)
        {
            if (step.textElement != null)
            {
                step.textElement.maxVisibleCharacters = 0;
            }
        }

        if (_actionButton != null) _actionButton.SetActive(false);
    }

    private void Start()
    {
        // Fetch the clip and volume from your Audiopool once at the start
        AudiopoolSFX.SFXData sfxData = AudiopoolSFX.Instance.GetSFXData(_typingSfxName);
        if (sfxData != null)
        {
            _audioSource.clip = sfxData.clip;
            _audioSource.volume = sfxData.volume;
        }

        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        foreach (var step in _sequence)
        {
            if (step.textElement == null) continue;

            // --- SFX TRIGGER ---
            // Start the continuous sound right before typing begins
            if (_audioSource.clip != null) _audioSource.Play();

            yield return StartCoroutine(TypeText(step.textElement));

            // --- SFX STOP ---
            // Stop the sound immediately when the line is finished
            _audioSource.Stop();

            yield return new WaitForSeconds(step.delayAfterFinished);
        }

        yield return new WaitForSeconds(_delayBeforeButton);
        if (_actionButton != null)
        {
            _actionButton.SetActive(true);
            AudiopoolSFX.Instance.Play("SFX_ButtonPress");
        }
    }

    private IEnumerator TypeText(TextMeshProUGUI target)
    {
        // Force the mesh update so characterInfo is populated
        target.ForceMeshUpdate();
        TMP_TextInfo textInfo = target.textInfo;
        int totalCharacters = textInfo.characterCount;

        float accumulatedTime = 0;
        int visibleCount = 0;
        float timePerChar = 1f / _charsPerSecond;

        while (visibleCount < totalCharacters)
        {
            // --- WHITESPACE SKIPPING LOGIC ---
            // If the current character is a space, tab, or newline, skip it instantly
            char currentChar = textInfo.characterInfo[visibleCount].character;

            if (char.IsWhiteSpace(currentChar))
            {
                visibleCount++;
                target.maxVisibleCharacters = visibleCount;
                // No 'yield return' here means it happens in a single frame
                continue;
            }

            // --- STANDARD TYPING LOGIC ---
            accumulatedTime += Time.deltaTime;
            if (accumulatedTime >= timePerChar)
            {
                visibleCount++;
                target.maxVisibleCharacters = visibleCount;
                accumulatedTime = 0;
            }

            yield return null;
        }

        // Final safety check to ensure all characters are visible (including trailing punctuation)
        target.maxVisibleCharacters = totalCharacters;
    }
}