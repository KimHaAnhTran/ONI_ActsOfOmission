using UnityEngine;
using System.Collections.Generic;

public class AudiopoolDialogue : MonoBehaviour
{
    public static AudiopoolDialogue Instance { get; private set; }
    private Dictionary<string, AudioClip> _dialogueCache = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Optional: Preload everything in that folder
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Audio/Pal Dialogue");
        foreach (var c in clips) _dialogueCache.Add(c.name, c);
    }

    public AudioClip GetClip(string clipName)
    {
        if (_dialogueCache.TryGetValue(clipName, out AudioClip clip)) return clip;
        return null;
    }
}