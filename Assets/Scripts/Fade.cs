using UnityEngine;
using System.Collections;

public class Fade : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _fadeSpeed = 1.0f;

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

        // Block clicks immediately
        if (_collider != null) _collider.enabled = true;

        // Ensure it sits above everything else
        _spriteRenderer.sortingOrder = 999;
    }

    public void StartFadeIn() // Screen goes from Black to Clear
    {
        StopCurrentFade();
        _fadeCoroutine = StartCoroutine(FadeRoutine(0.0f));
    }

    public void StartFadeOut() // Screen goes from Clear to Black
    {
        StopCurrentFade();
        _fadeCoroutine = StartCoroutine(FadeRoutine(1.0f));
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        // If we are moving TO black, block clicks immediately
        if (targetAlpha > 0.5f && _collider != null) _collider.enabled = true;

        Color currentColor = _spriteRenderer.color;

        while (!Mathf.Approximately(currentColor.a, targetAlpha))
        {
            currentColor.a = Mathf.MoveTowards(currentColor.a, targetAlpha, _fadeSpeed * Time.deltaTime);
            _spriteRenderer.color = currentColor;
            yield return null;
        }

        // If we are now Clear, unblock the screen
        if (targetAlpha < 0.1f && _collider != null) _collider.enabled = false;
    }

    private void StopCurrentFade()
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
    }
}