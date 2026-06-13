using UnityEngine;
using ActiveSaga.RunGame;

public class CoinCollectible : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int coinValue = 1;
    [SerializeField] private float rotateSpeed = 120f;

    [Header("Audio")]
    [SerializeField] private AudioClip collectSound;

    private RunGameStatsTracker statsTracker;
    private bool collected;

    public void Initialize(RunGameStatsTracker tracker)
    {
        statsTracker = tracker;
        collected = false;
    }

    private void Update()
    {
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        collected = true;

        if (statsTracker != null)
        {
            statsTracker.AddCoins(coinValue);
        }

        if (ActiveSagaAudioManager.Instance != null)
        {
            ActiveSagaAudioManager.Instance.PlaySFX(collectSound);
        }

        Destroy(gameObject);
    }
}