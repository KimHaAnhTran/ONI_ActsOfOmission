using UnityEngine;
using System.Collections;

public class ParcelLate : MonoBehaviour
{

    [Header("Paper slide duration")]
    [SerializeField] private float _duration = 1f;

    private void OnEnable()
    {
        // Listen for the timer running out
        TimerManager.OnTimerRanOut += HandleLateEvent;
    }

    private void OnDisable()
    {
        TimerManager.OnTimerRanOut -= HandleLateEvent;
    }

    private void HandleLateEvent()
    {
        StartCoroutine(PullDocumentsLeftRoutine());
    }

    private IEnumerator PullDocumentsLeftRoutine()
    {
        AudiopoolSFX.Instance.Play("SFX_PaperSlide");

        // Find all active documents on the desk
        GameObject[] documents = GameObject.FindGameObjectsWithTag("Document");

        float elapsed = 0f;

        Vector3[] docStarts = new Vector3[documents.Length];
        for (int i = 0; i < documents.Length; i++) docStarts[i] = documents[i].transform.position;

        // Move them far to the Left
        Vector3 moveOffset = Vector3.left * 6.5f;

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _duration;

            for (int i = 0; i < documents.Length; i++)
            {
                if (documents[i] != null)
                    documents[i].transform.position = Vector3.Lerp(docStarts[i], docStarts[i] + moveOffset, t);
            }

            yield return null;
        }

        // Cleanup the old documents
        foreach (GameObject doc in documents) Destroy(doc);

        // Wait half a second for the paper fade exit to settle
        yield return new WaitForSeconds(0.5f);

        // --- VOICEMAIL CHECK ---
        // Look for a voicemail labeled "Late" (e.g., Day1_Doc1_Late.mp3 and .txt)
        bool hasVoicemail = PalReactionsController.Instance.TryTriggerReaction("Late");

        // If no late voicemail exists, immediately spawn the next document batch
        // (If it DOES exist, PalReactionsController will handle spawning the next batch after the audio finishes)
        if (!hasVoicemail)
        {
            GenerateDocument.OnSpawnNextBatch?.Invoke();
        }
    }
}