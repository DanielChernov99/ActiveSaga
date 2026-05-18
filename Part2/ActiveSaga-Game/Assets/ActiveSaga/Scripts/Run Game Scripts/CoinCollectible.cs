using UnityEngine;
using ActiveSaga.RunGame;

public class CoinCollectible : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int coinValue = 1;
    [SerializeField] private float rotateSpeed = 120f;

    private RunGameStatsTracker statsTracker;

    public void Initialize(RunGameStatsTracker tracker)
    {
        statsTracker = tracker;
    }

    private void Update()
    {
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
            Debug.Log("Coin touched by: " + other.name + ", tag: " + other.tag);

        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (statsTracker != null)
        {
            statsTracker.AddCoins(coinValue);
        }

        Destroy(gameObject);
    }
}