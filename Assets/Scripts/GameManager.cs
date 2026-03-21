using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Static instance that other scripts can access via GameManager.Instance
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // --- SINGLETON LOGIC ---
        if (Instance == null)
        {
            Instance = this;
            // This prevents the object from being deleted when a new scene loads
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If another GameManager already exists (e.g., when loading into Day 2), 
            // destroy this new one immediately so we only ever have ONE.
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Optional: Ensure MainDataset checks the day as soon as the game starts
        MainDataset.CheckDay();
    }

    // Example method to change scenes from code
    public void LoadNextDay(string sceneName)
    {
        // You could trigger your fade-out animation here first
        SceneManager.LoadScene(sceneName);
    }
}