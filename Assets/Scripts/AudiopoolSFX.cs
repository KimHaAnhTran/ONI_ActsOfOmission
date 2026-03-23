using UnityEngine;
using System.Collections.Generic;

public class AudiopoolSFX : MonoBehaviour
{
    public static AudiopoolSFX Instance { get; private set; }

    [System.Serializable]
    public class SFXData
    {
        public string fileName;      // Exact name in Assets/Resources/Audio/SFX
        [Range(0f, 1f)]
        public float volume = 1f;    // Individual volume slider
        [HideInInspector]
        public AudioClip clip;       // The actual file loaded from Resources
    }

    [Header("SFX Library Settings")]
    [SerializeField] private List<SFXData> _sfxList = new List<SFXData>();

    // Shortcuts for easy access in other scripts
    // These strings must match the 'fileName' you type in the Inspector
    private Dictionary<string, SFXData> _sfxDictionary = new Dictionary<string, SFXData>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        InitializeLibrary();
    }

    private void InitializeLibrary()
    {
        // The path relative to the 'Resources' folder
        string basePath = "Audio/SFX/";

        foreach (SFXData sfx in _sfxList)
        {
            if (string.IsNullOrEmpty(sfx.fileName)) continue;

            // Load from: Assets/Resources/Audio/SFX/fileName
            sfx.clip = Resources.Load<AudioClip>(basePath + sfx.fileName);

            if (sfx.clip == null)
            {
                Debug.LogError($"<color=red>AudiopoolSFX:</color> Could not find {sfx.fileName} at Resources/{basePath}");
                continue;
            }

            // Add to dictionary for fast lookup by name
            if (!_sfxDictionary.ContainsKey(sfx.fileName))
            {
                _sfxDictionary.Add(sfx.fileName, sfx);
            }
        }

        Debug.Log($"<color=cyan>AudiopoolSFX:</color> Successfully loaded {_sfxDictionary.Count} clips.");
    }

    /// <summary>
    /// Play a sound by its filename. Use the exact name from the Resources folder.
    /// </summary>
    public void Play(string sfxName)
    {
        if (_sfxDictionary.TryGetValue(sfxName, out SFXData data))
        {
            if (data.clip != null)
            {
                // PlayClipAtPoint creates a temporary object that plays the sound
                // Using Camera.main.transform.position ensures it's heard clearly in 2D
                AudioSource.PlayClipAtPoint(data.clip, Camera.main.transform.position, data.volume);
            }
        }
        else
        {
            Debug.LogWarning($"AudiopoolSFX: Sound '{sfxName}' not found in library!");
        }
    }

    // A getter if you need the raw clip for a dedicated AudioSource (like the typewriter)
    public SFXData GetSFXData(string sfxName)
    {
        _sfxDictionary.TryGetValue(sfxName, out SFXData data);
        return data;
    }
}