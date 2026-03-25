using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenResolutionSetter : MonoBehaviour
{
    private static GameObject _persistentBlackBarCam;

    // --- NEW: Track the screen size ---
    private int _lastWidth;
    private int _lastHeight;

    private void Awake()
    {
        // 1. Handle the Black Bar Camera (Singleton Pattern)
        if (_persistentBlackBarCam == null)
        {
            _persistentBlackBarCam = new GameObject("Global_BlackBarCamera");
            Camera backCam = _persistentBlackBarCam.AddComponent<Camera>();
            backCam.clearFlags = CameraClearFlags.SolidColor;
            backCam.backgroundColor = Color.black;
            backCam.cullingMask = 0;
            backCam.depth = -100;
            DontDestroyOnLoad(_persistentBlackBarCam);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Record initial size and apply immediately
        _lastWidth = Screen.width;
        _lastHeight = Screen.height;
        ApplyToCurrentCamera();
    }

    // --- NEW: Detect Fullscreen / Resizing in real-time ---
    private void Update()
    {
        // If the browser window resizes or the player enters Fullscreen, recalculate!
        if (Screen.width != _lastWidth || Screen.height != _lastHeight)
        {
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;
            ApplyToCurrentCamera();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyToCurrentCamera();
    }

    private void ApplyToCurrentCamera()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            // --- NEW: Platform Detection ---
#if UNITY_WEBGL
            // This code ONLY compiles when building for WebGL
            // It ensures the 16:9 ratio is forced inside the browser's fullscreen canvas
            ApplyAspectRatio(16f, 9f, cam);

#elif UNITY_STANDALONE
                // This code ONLY compiles when building for Windows/Mac/Linux .exe
                ApplyAspectRatio(16f, 9f, cam);
                
#else
                // Fallback for Editor or other platforms
                ApplyAspectRatio(16f, 9f, cam);
#endif
        }
    }

    public void ApplyAspectRatio(float width, float height, Camera cam)
    {
        float targetAspect = width / height;
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1.0f)
        {
            Rect rect = cam.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            cam.rect = rect;
        }
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = cam.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            cam.rect = rect;
        }
    }
}