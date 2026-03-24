using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenResolutionSetter : MonoBehaviour
{
    private static GameObject _persistentBlackBarCam;

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
        // Apply immediately to the current scene camera
        ApplyToCurrentCamera();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Apply every time a new scene is loaded
        ApplyToCurrentCamera();
    }

    private void ApplyToCurrentCamera()
    {
        Camera cam = Camera.main; // Finds the camera tagged "MainCamera"
        if (cam != null)
        {
            ApplyAspectRatio(16f, 9f, cam);
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