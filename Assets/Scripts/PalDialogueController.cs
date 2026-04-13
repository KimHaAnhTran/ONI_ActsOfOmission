using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[RequireComponent(typeof(AudioSource))]
public class PalDialogueController : MonoBehaviour
{
    public static PalDialogueController Instance { get; private set; }

    [System.Serializable]
    public class DialogueLine
    {
        public float startTime;
        public string text;
    }

    [Header("UI References")]
    [SerializeField] private RectTransform _backgroundBox;
    [SerializeField] private TextMeshProUGUI _dialogueText;

    [Header("Settings")]
    [SerializeField] private Vector2 _padding = new Vector2(40f, 20f);
    // Use this to limit how wide the box can grow before it starts wrapping text
    [SerializeField] private float _maxWidth = 600f;

    // Adjustable volume control defaulting to 0.5f
    [Range(0f, 1f)]
    private float _audioVolume = 0.5f;

    private AudioSource _audioSource;
    private List<DialogueLine> _currentLines = new List<DialogueLine>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _audioSource = GetComponent<AudioSource>();
            _backgroundBox.gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TriggerDialogue(string fileName)
    {
        StopAllCoroutines();

        AudiopoolSFX.Instance.Play("SFX_CassetteStart");
        

        AudioClip voiceClip = Resources.Load<AudioClip>("Audio/Pal Dialogue/" + fileName);
        // Changed path to "Audio Transcript" as per your TriggerDialogue logic
        TextAsset transcript = Resources.Load<TextAsset>("Audio Transcript/" + fileName);

        if (voiceClip == null || transcript == null)
        {
            Debug.LogError($"PalDialogue: Missing files for {fileName}");
            return;
        }

        ParseTranscript(transcript.text);
        StartCoroutine(ExecuteDialogue(voiceClip));
    }

    private void ParseTranscript(string rawText)
    {
        _currentLines.Clear();
        string timePattern = @"[\(\[](\d+):(\d+)[\)\]]";
        string[] textSegments = Regex.Split(rawText, timePattern);
        MatchCollection matches = Regex.Matches(rawText, timePattern);

        for (int i = 0; i < matches.Count; i++)
        {
            float minutes = float.Parse(matches[i].Groups[1].Value);
            float seconds = float.Parse(matches[i].Groups[2].Value);
            float totalTime = (minutes * 60) + seconds;

            int textIndex = (i * 3) + 3;
            if (textIndex < textSegments.Length)
            {
                _currentLines.Add(new DialogueLine
                {
                    startTime = totalTime,
                    text = textSegments[textIndex].Trim()
                });
            }
        }
    }

    private IEnumerator ExecuteDialogue(AudioClip clip)
    {
        _backgroundBox.gameObject.SetActive(true);
        _audioSource.clip = clip;

        // Apply the volume before playing
        _audioSource.volume = _audioVolume;

        _audioSource.Play();

        int lineIndex = 0;
        // This keeps the loop alive if the audio is paused (because time will be > 0)
        while (_audioSource.isPlaying || _audioSource.time > 0f)
        {
            float currentTime = _audioSource.time;
            if (lineIndex < _currentLines.Count && currentTime >= _currentLines[lineIndex].startTime)
            {
                UpdateUI(_currentLines[lineIndex].text);
                lineIndex++;
            }
            yield return null;
        }

        _backgroundBox.sizeDelta = new Vector2(0, _backgroundBox.sizeDelta.y);
        _backgroundBox.gameObject.SetActive(false);
        _dialogueText.text = "";

        // --- TRIGGER THE DOCUMENTS ---
        // This tells GenerateDocument to spawn the first batch now that the talk is over
        AudiopoolSFX.Instance.Play("SFX_CassetteStop");
        yield return new WaitForSeconds(1f);
        GenerateDocument.OnSpawnNextBatch?.Invoke();
    }

    private void UpdateUI(string content)
    {
        // 1. Set the text
        _dialogueText.text = content;

        // 2. Set the text container size constraints so wrapping works correctly
        // (X = maxWidth, Y = 0 tells TMPro to grow vertically as much as needed)
        _dialogueText.rectTransform.sizeDelta = new Vector2(_maxWidth, 0);

        // 3. FORCE the mesh update to calculate actual character positions
        _dialogueText.ForceMeshUpdate();

        // 4. Use textBounds.size for the most accurate rendered dimensions
        Vector2 renderedSize = _dialogueText.textBounds.size;

        // 5. Apply the size to the background box + padding
        _backgroundBox.sizeDelta = new Vector2(renderedSize.x + _padding.x, renderedSize.y + _padding.y);

        // 6. Handle the Z-position and Centering
        // We get the current local position, set Z to 0, and then apply it back.
        Vector3 targetLocalPos = _dialogueText.rectTransform.localPosition;
        targetLocalPos.z = 0;
        _dialogueText.rectTransform.localPosition = targetLocalPos;
    }
}