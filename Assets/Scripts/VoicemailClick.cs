using UnityEngine;
using System.Collections;

public class VoicemailClick : MonoBehaviour
{
    [Header("Bounce Settings")]
    [SerializeField] private float _bounceHeight = 0.5f;
    [SerializeField] private float _bounceDuration = 0.4f;

    [Header("Hover Settings")]
    [SerializeField] private float _hoverYOffset = 0.2f;
    [SerializeField] private float _hoverSpeed = 10f;

    [Header("Exit Settings")]
    [SerializeField] private Vector3 _exitOffset = new Vector3(0, 10f, 0); // Where it flies to
    [SerializeField] private float _exitDuration = 0.8f;

    private bool _isClicked = false;
    private Vector3 _basePosition;
    private Coroutine _hoverCoroutine;

    private void Start()
    {
        // Store the original position to return to after hovering
        _basePosition = transform.position;
    }

    private void OnMouseEnter()
    {
        // Cancel hover if already clicked/exiting
        if (_isClicked) return;

        AudiopoolSFX.Instance.Play("SFX_PaperFolds");

        StopHover();
        _hoverCoroutine = StartCoroutine(MoveToPosition(_basePosition + new Vector3(0, _hoverYOffset, 0)));
    }

    private void OnMouseExit()
    {
        // Cancel hover if already clicked/exiting
        if (_isClicked) return;

        StopHover();
        _hoverCoroutine = StartCoroutine(MoveToPosition(_basePosition));
    }

    private void OnMouseDown()
    {
        // Prevent double-clicking while the animation is playing
        if (_isClicked) return;
        _isClicked = true;

        AudiopoolSFX.Instance.Play("SFX_ButtonPress");

        StopHover(); // Stop any active hover lerp immediately
        StartCoroutine(AnimateAndTrigger());
    }

    private IEnumerator MoveToPosition(Vector3 target)
    {
        // Smooth lerp for hover feedback
        while (Vector3.Distance(transform.position, target) > 0.001f)
        {
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * _hoverSpeed);
            yield return null;
        }
        transform.position = target;
    }

    private void StopHover()
    {
        if (_hoverCoroutine != null) StopCoroutine(_hoverCoroutine);
    }

    private IEnumerator AnimateAndTrigger()
    {
        // Use current position as start to avoid snapping if player clicks while hovering
        Vector3 startPos = transform.position;

        // --- PHASE 1: THE BOUNCE ---
        float elapsed = 0f;
        while (elapsed < _bounceDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _bounceDuration;

            // Use Sine to move up and then back down to startPos
            float yOffset = Mathf.Sin(t * Mathf.PI) * _bounceHeight;
            transform.position = startPos + new Vector3(0, yOffset, 0);

            yield return null;
        }

        // --- PHASE 2: THE EXIT ---
        elapsed = 0f;
        Vector3 currentPos = transform.position;
        Vector3 targetPos = currentPos + _exitOffset;

        AudiopoolSFX.Instance.Play("SFX_PaperSlide");

        while (elapsed < _exitDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _exitDuration;

            // SmoothStep for a polished "Ease Out" effect
            t = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(currentPos, targetPos, t);
            yield return null;
        }

        // --- PHASE 3: TRIGGER DIALOGUE & DESTROY ---
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TriggerDayDialogue();
        }
        else
        {
            Debug.LogError("VoicemailClick.cs: GameManager Instance not found!");
        }


        Destroy(gameObject);
    }
}