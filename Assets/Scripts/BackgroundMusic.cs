using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic Instance { get; private set; }

    [Header("Audio Settings")]
    [SerializeField] private AudioClip _musicTrack;
    [Range(0f, 1f)]
    [SerializeField] private float _volume = 0.04f;
    [SerializeField] private bool _playOnAwake = true;

    private AudioSource _audioSource;

    private void Awake()
    {
        // Keep the music playing across scene changes
        if (Instance == null)
        {
            Instance = this;
            // Safety check: DontDestroyOnLoad only works on root objects
            transform.parent = null;

            DontDestroyOnLoad(gameObject);
            SetupAudioSource();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void SetupAudioSource()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.clip = _musicTrack;
        _audioSource.loop = true;
        _audioSource.volume = _volume;
        _audioSource.playOnAwake = false;

        if (_playOnAwake && _musicTrack != null)
        {
            _audioSource.Play();
        }
    }

    // This allows you to adjust volume via the slider in Inspector during runtime
    private void OnValidate()
    {
        if (_audioSource != null)
        {
            _audioSource.volume = _volume;
        }
    }

    public void SetVolume(float newVolume)
    {
        _volume = Mathf.Clamp01(newVolume);
        if (_audioSource != null) _audioSource.volume = _volume;
    }
}