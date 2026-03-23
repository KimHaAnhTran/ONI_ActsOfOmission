using UnityEngine;
using System.Collections;

public class Fade : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _fadeDuration = 1.5f; // How long the actual transition takes
    [SerializeField] private float _fadeDelay = 1.5f;    // How long to wait before starting

    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _collider;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<BoxCollider2D>();

        // FORCE BLACK IMMEDIATELY
        Color c = _spriteRenderer.color;
        c.a = 1f;
        _spriteRenderer.color = c;

        // Block clicks immediately at scene start
        if (_collider != null) _collider.enabled = true;
    }

    public void StartFadeIn() // Black to Clear
    {
        StopCurrentFade();
        _fadeCoroutine = StartCoroutine(FadeRoutine(0.0f));
    }

    public void StartFadeOut() // Clear to Black
    {
        StopCurrentFade();
        _fadeCoroutine = StartCoroutine(FadeRoutine(1.0f));
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        // 1. If moving TO black, block clicks BEFORE the delay
        if (targetAlpha > 0.5f && _collider != null) _collider.enabled = true;

        // 2. Initial Delay
        yield return new WaitForSeconds(_fadeDelay);

        float startAlpha = _spriteRenderer.color.a;
        float elapsedTime = 0f;

        // 3. Smooth Lerp Loop
        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += Time.deltaTime;

            // Calculate progress (0 to 1)
            float t = elapsedTime / _fadeDuration;

            // Apply SmoothStep to the 't' value for extra "Ease In/Out" smoothness
            t = t * t * (3f - 2f * t);

            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            Color c = _spriteRenderer.color;
            c.a = newAlpha;
            _spriteRenderer.color = c;

            yield return null;
        }

        // Ensure we hit the exact target at the end
        Color finalColor = _spriteRenderer.color;
        finalColor.a = targetAlpha;
        _spriteRenderer.color = finalColor;

        // 4. If finished fading TO Clear, unblock the screen
        if (targetAlpha < 0.1f && _collider != null) _collider.enabled = false;
    }

    private void StopCurrentFade()
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
    }
}