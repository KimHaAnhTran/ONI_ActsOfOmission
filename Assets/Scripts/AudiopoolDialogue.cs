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

        // Preload everything in that folder
        AudioClip[] clips = Resources.LoadAll<AudioClip>("Audio/Pal Dialogue");
        foreach (var c in clips)
        {
            // Safe Add
            // Check if the dictionary already has this name before adding it
            if (!_dialogueCache.ContainsKey(c.name))
            {
                _dialogueCache.Add(c.name, c);
            }
            else
            {
                // This won't crash the game, but will warn in the console
                Debug.LogWarning($"AudiopoolDialogue: Found a duplicate audio file named '{c.name}'! Ignoring the duplicate.");
            }
        }
    }

    public AudioClip GetClip(string clipName)
    {
        if (_dialogueCache.TryGetValue(clipName, out AudioClip clip)) return clip;
        return null;
    }
}