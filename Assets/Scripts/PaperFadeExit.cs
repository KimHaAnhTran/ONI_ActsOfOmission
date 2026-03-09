using UnityEngine;
using System.Collections;

public class PaperFadeExit : MonoBehaviour
{
    [Header("Exit Animation")]
    [SerializeField] private float _exitSpeed = 1f;
    [SerializeField] private float _fadeDuration = 0.5f;
    [SerializeField] private Vector3 _moveDirection = Vector3.up;

    [Header("Replacement")]
    [SerializeField] private GameObject _parcelSmallPrefab;
    [SerializeField] private GameObject _parcelPrefab;

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
        if (_parcelSmallPrefab != null && _parcelPrefab != null)
        {
            Vector3 newPosSmall = transform.position + new Vector3(0f, 0.5f, 0f);
            Vector3 newPos = transform.position + new Vector3(0f, 2f, 0f);
            // We instantiate it inside the Right Desk parent, not inside the typewriter
            GameObject parent = GameObject.Find("LeftDesk");

            GameObject smallParcel = Instantiate(_parcelSmallPrefab, newPosSmall, transform.rotation, transform.parent.transform.parent);
            smallParcel.name = "Parcel_S";

            GameObject bigParcel = Instantiate(_parcelPrefab, newPos, transform.rotation, parent.transform);
            bigParcel.name = "Parcel";

            smallParcel.GetComponent<SwitchDocBetweenScreens>().SetPairDoc(bigParcel);
            bigParcel.GetComponent<SwitchDocBetweenScreens>().SetPairDoc(smallParcel);
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