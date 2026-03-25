using UnityEngine;
using System.Collections;

public class VoicemailReaction : MonoBehaviour
{
    public static VoicemailReaction Instance { get; private set; }

    [Header("Position Settings")]
    [SerializeField] private Transform _yHidden;
    [SerializeField] private Transform _yVisible;
    [SerializeField] private float _hoverOffset = 0.2f;

    [Header("Animation Settings")]
    [SerializeField] private float _smoothTime = 0.2f;
    [SerializeField] private float _bounceHeight = 0.3f;
    [SerializeField] private float _bounceDuration = 0.4f;

    private float _currentTargetY;
    private float _currentVelocity;
    private bool _isActive = false;
    private bool _isClicked = false;
    private bool _isMouseOver = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Start hidden
        transform.localPosition = new Vector3(transform.localPosition.x, _yHidden.position.y, transform.localPosition.z);
        _currentTargetY = _yHidden.position.y;
    }

    // Called by PalReactionsController
    public void PopUp()
    {
        AudiopoolSFX.Instance.Play("SFX_CassetteClick"); // Or an "Incoming Call" sound
        _isClicked = false;
        _isActive = true;
        _currentTargetY = _yVisible.position.y;
    }

    private void Update()
    {
        // Handle the smooth Damping logic manually to avoid Coroutine overlap
        if (!_isClicked)
        {
            float newY = Mathf.SmoothDamp(transform.localPosition.y, _currentTargetY, ref _currentVelocity, _smoothTime);
            transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
        }
    }

    private void OnMouseEnter()
    {
        if (!_isActive || _isClicked) return;
        _isMouseOver = true;
        _currentTargetY = _yVisible.position.y + _hoverOffset;
        AudiopoolSFX.Instance.Play("SFX_PaperDragDrop");
    }

    private void OnMouseExit()
    {
        if (!_isActive || _isClicked) return;
        _isMouseOver = false;
        _currentTargetY = _yVisible.position.y;
    }

    private void OnMouseDown()
    {
        if (!_isActive || _isClicked) return;

        _isClicked = true;
        _isActive = false;
        AudiopoolSFX.Instance.Play("SFX_CassetteClick");

        StartCoroutine(BounceAndHideRoutine());
    }

    private IEnumerator BounceAndHideRoutine()
    {
        Vector3 startPos = transform.localPosition;
        float elapsed = 0f;

        // 1. The Downward/Upward Bounce
        while (elapsed < _bounceDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _bounceDuration;

            // A quick physical "press down" then snap back
            float yOffset = Mathf.Sin(t * Mathf.PI) * -_bounceHeight;
            transform.localPosition = startPos + new Vector3(0, yOffset, 0);
            yield return null;
        }

        // 2. Slide back to hidden position
        _currentTargetY = _yHidden.position.y;
        while (Mathf.Abs(transform.localPosition.y - _currentTargetY) > 0.01f)
        {
            float newY = Mathf.SmoothDamp(transform.localPosition.y, _currentTargetY, ref _currentVelocity, _smoothTime);
            transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
            yield return null;
        }

        yield return new WaitForSeconds(0.25f);
        // 3. Tell the controller to start talking immediately after click
        PalReactionsController.Instance.PlayReaction();
    }
}