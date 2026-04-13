using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

[RequireComponent(typeof(AudioSource))]
public class PalReactionsController : MonoBehaviour
{
    public static PalReactionsController Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private RectTransform _backgroundBox; // Changed to RectTransform for resizing
    [SerializeField] private TextMeshProUGUI _subtitleText;

    [Header("Settings")]
    [SerializeField] private Vector2 _padding = new Vector2(0.2f, 0.01f);
    [SerializeField] private float _maxWidth = 3.5f;

    [Range(0f, 1f)]
    private float _audioVolume = 0.5f;

    // Adjustable volume control defaulting to 0.5f
    private AudioSource _audioSource;
    private AudioClip _pendingAudio;

    private struct SubtitleData
    {
        public float Time;
        public string Text;
    }
    private List<SubtitleData> _currentSubtitles = new List<SubtitleData>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        _audioSource = GetComponent<AudioSource>();
        _backgroundBox.gameObject.SetActive(false);
    }

    // Checks if files exist for this decision. If they do, arms the voicemail and returns true.
    public bool TryTriggerReaction(string decisionAction)
    {
        // 1. Generate the exact file name
        string fileName = $"Day{MainDataset.GetGroupIndex() + 1}_Doc{GenerateDocument.GetCurrentIndex()}_{decisionAction}";

        // 2. Build the exact paths
        string audioPath = $"Audio/Pal Dialogue/Send Discard/{fileName}";
        string textPath = $"Audio Transcript/Send Discard/{fileName}";

        // 3. Tell the console what we are looking for
        Debug.Log($"[Voicemail Check] Looking for Audio: {audioPath}");
        Debug.Log($"[Voicemail Check] Looking for Text: {textPath}");

        // 4. Attempt to load
        AudioClip audioClip = Resources.Load<AudioClip>(audioPath);
        TextAsset textAsset = Resources.Load<TextAsset>(textPath);

        // 5. Tell the console if either one failed
        if (audioClip == null) Debug.LogWarning($"[Voicemail Error] Could not find Audio file!");
        if (textAsset == null) Debug.LogWarning($"[Voicemail Error] Could not find Text file! (Make sure it's a true .txt file)");

        // 6. Proceed if BOTH are found
        if (audioClip != null && textAsset != null)
        {
            _pendingAudio = audioClip;
            ParseSubtitles(textAsset.text);

            if (VoicemailReaction.Instance != null)
            {
                VoicemailReaction.Instance.PopUp();
            }
            else
            {
                Debug.LogError("PalReactionsController: VoicemailReaction Instance not found in scene!");
            }
            return true;
        }

        return false; // No reaction found, proceed normally
    }

    private void ParseSubtitles(string rawText)
    {
        _currentSubtitles.Clear();
        string[] lines = rawText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string line in lines)
        {
            if (line.StartsWith("("))
            {
                int closeBracket = line.IndexOf(')');
                if (closeBracket > 0)
                {
                    string timeStr = line.Substring(1, closeBracket - 1);
                    string[] timeParts = timeStr.Split(':');
                    float timeSeconds = 0f;

                    if (timeParts.Length == 2)
                    {
                        timeSeconds = (int.Parse(timeParts[0]) * 60f) + int.Parse(timeParts[1]);
                    }

                    string textStr = line.Substring(closeBracket + 1).Trim();
                    _currentSubtitles.Add(new SubtitleData { Time = timeSeconds, Text = textStr });
                }
            }
        }
    }

    public void PlayReaction()
    {
        if (_pendingAudio != null)
        {
            StartCoroutine(ReactionRoutine());
        }
    }

    private IEnumerator ReactionRoutine()
    {
        _backgroundBox.gameObject.SetActive(true);
        UpdateSubtitleBox(""); // Clear text initially

        _audioSource.clip = _pendingAudio;

        // Apply the volume before playing
        _audioSource.volume = _audioVolume;
        _audioSource.Play();

        // Attempt to get volume from Audiopool if you have it set there, otherwise default to 1
        _audioSource.Play();

        int subIndex = 0;

        // --- Loop condition to prevent UI Rect and subtitle from disappearing when exit game ---
        while (_audioSource.isPlaying || _audioSource.time > 0f)
        {
            float currentTime = _audioSource.time;

            while (subIndex < _currentSubtitles.Count && currentTime >= _currentSubtitles[subIndex].Time)
            {
                UpdateSubtitleBox(_currentSubtitles[subIndex].Text);
                subIndex++;
            }

            yield return null;
        }

        _backgroundBox.gameObject.SetActive(false);
        UpdateSubtitleBox("");
        _pendingAudio = null;

        // --- TRIGGER THE DOCUMENTS ---
        // This tells GenerateDocument to spawn the next batch now that the talk is over
        AudiopoolSFX.Instance.Play("SFX_CassetteStop");
        yield return new WaitForSeconds(1f);
        GenerateDocument.OnSpawnNextBatch?.Invoke();
    }

    // Dynamically resizes the text and background box based on MaxWidth and Padding
    private void UpdateSubtitleBox(string newText)
    {
        _subtitleText.text = newText;
        _subtitleText.ForceMeshUpdate(); // Force TMP to calculate dimensions immediately

        // 1. Get the width of the text, but don't let it exceed MaxWidth
        float textWidth = Mathf.Min(_subtitleText.preferredWidth, _maxWidth);

        // 2. Get the height of the text based on that clamped width (handles wrapping)
        Vector2 preferredSize = _subtitleText.GetPreferredValues(newText, textWidth, Mathf.Infinity);
        float textHeight = preferredSize.y;

        // 3. Apply the exact size to the Text RectTransform
        _subtitleText.rectTransform.sizeDelta = new Vector2(textWidth, textHeight);

        // 4. Apply the size + padding to the Background RectTransform
        if (_backgroundBox != null)
        {
            _backgroundBox.sizeDelta = new Vector2(textWidth + _padding.x, textHeight + _padding.y);
        }
    }
}