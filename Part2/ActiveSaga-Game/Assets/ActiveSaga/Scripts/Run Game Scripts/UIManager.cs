using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Game Manager Connection")]
    [SerializeField] private GameManager gameManager;

    [Header("Run HUD Texts")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI distanceText;

    private void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (gameManager == null)
        {
            Debug.LogError("UIManager: GameManager is missing.");
            return;
        }

        gameManager.OnStatsUpdated += HandleGameUpdate;

        HandleGameUpdate(0f, 0, 0, 0f);
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnStatsUpdated -= HandleGameUpdate;
        }
    }

    private void HandleGameUpdate(float distance, int jumps, int squats, float elapsedTime)
    {
        if (timerText != null)
        {
            timerText.text = "Time: " + FormatTime(elapsedTime);
        }

        if (distanceText != null)
        {
            distanceText.text = "Distance: " + distance.ToString("0") + " m";
        }
    }

    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int remainingSeconds = Mathf.FloorToInt(seconds % 60f);

        return minutes.ToString("00") + ":" + remainingSeconds.ToString("00");
    }
}