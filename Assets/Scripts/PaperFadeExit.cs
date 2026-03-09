using UnityEngine;
using System.Collections;

public class PaperFadeExit : MonoBehaviour
{
    [Header("Exit Animation")]
    [SerializeField] private float _exitSpeed = 1f;
    [SerializeField] private float _fadeDuration = 0.5f;
    [SerializeField] private Vector3 _moveDirection = Vector3.up;

    [Header("Replacement")]
    [SerializeField] private GameObject _finishedPaperPrefab;

    private SpriteRenderer _spriteRenderer;
    private bool _isExiting = false;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        TypewriterKey.OnCanTypeChanged += HandleExitTrigger;
    }

    private void OnDisable()
    {
        TypewriterKey.OnCanTypeChanged -= HandleExitTrigger;
    }

    private void HandleExitTrigger(bool canType)
    {
        // When CanType becomes false, it means the document is done
        if (!canType && !_isExiting)
        {
            StartCoroutine(ExitRoutine());
        }
    }

    private IEnumerator ExitRoutine()
    {
        _isExiting = true;

        // 1. Spawn the replacement object at the current position
        if (_finishedPaperPrefab != null)
        {
            // We instantiate it inside the Right Desk parent, not inside the typewriter
            Instantiate(_finishedPaperPrefab, transform.position, transform.rotation, transform.parent.transform.parent);
        }

        float elapsed = 0f;
        Color originalColor = _spriteRenderer.color;

        while (elapsed < _fadeDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / _fadeDuration;

            // Move Upwards
            transform.Translate(_moveDirection * _exitSpeed * Time.deltaTime);

            // Fade Out
            if (_spriteRenderer != null)
            {
                float newAlpha = Mathf.Lerp(1f, 0f, normalizedTime);
                _spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, newAlpha);
            }

            yield return null;
        }

        // 3. Goodbye!
        Destroy(gameObject);
    }
}