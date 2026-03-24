using UnityEngine;

public class ScreenResolutionSetter : MonoBehaviour
{
    private void Start()
    {
        ApplyAspectRatio(16f, 9f);
    }

    public void ApplyAspectRatio(float width, float height)
    {
        // 1. Determine the target aspect ratio
        float targetAspect = width / height;

        // 2. Determine the current window aspect ratio
        float windowAspect = (float)Screen.width / (float)Screen.height;

        // 3. Current viewport height should be scaled by this amount
        float scaleHeight = windowAspect / targetAspect;

        // 4. Get the camera component
        Camera camera = GetComponent<Camera>();

        // 5. If window is wider than target (Pillarbox)
        if (scaleHeight < 1.0f)
        {
            Rect rect = camera.rect;

            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;

            camera.rect = rect;
        }
        // 6. If window is taller than target (Letterbox)
        else
        {
            float scaleWidth = 1.0f / scaleHeight;

            Rect rect = camera.rect;

            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;

            camera.rect = rect;
        }
    }
}