using UnityEngine;
using TMPro;
using ActiveSaga.BossFight.Core;

public class UIManager : MonoBehaviour
{
    [Header("Game Manager Connection")]
    [SerializeField] private GameManager gameManager;

    [Header("Modules")]
    [Tooltip("The Progress Bar Logic")]
    [SerializeField] private RunningProgressBar progressBar;
    
    [Tooltip("The Left Side Panel Stats")]
    [SerializeField] private StatsDisplay statsDisplay; 

    [Header("Wave UI")]
    [SerializeField] private TextMeshProUGUI waveText;

    private void OnEnable()
    {
        EventManager.Subscribe<WaveStartedEvent>(OnWaveStarted);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<WaveStartedEvent>(OnWaveStarted);
    }

    private void OnWaveStarted(WaveStartedEvent e)
    {
        if (waveText != null)
        {
            waveText.text = $"Wave: {e.waveIndex}";
        }
    }

    private void Start()
    {
        if (gameManager != null)
        {
            // Subscribe to the event
            gameManager.OnStatsUpdated += HandleGameUpdate;
            
            // 1. New: Send the goals (Target Distance, Jumps, Squats) to the display
            if (statsDisplay != null)
            {
                statsDisplay.SetGoals(
                    gameManager.levelTargetDistance, 
                    gameManager.goalJumps, 
                    gameManager.goalSquats
                );
            }

            // Initial reset (Sending 0 time as well)
            HandleGameUpdate(0, 0, 0, 0f);
        }
        else
        {
            Debug.LogError("UIManager: Game Manager is missing!");
        }
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnStatsUpdated -= HandleGameUpdate;
        }
    }

    // Updated Signature: Now accepts 4 parameters (timeElapsed added at the end)
    private void HandleGameUpdate(float currentDist, int jumps, int squats, float timeElapsed)
    {
        // 1. Module A: Progress Bar (Logic preserved)
        if (progressBar != null)
        {
            float maxDist = gameManager.levelTargetDistance;
            
            // Protect against division by zero
            float progress = (maxDist > 0) ? (currentDist / maxDist) : 0;
            
            progressBar.UpdateVisuals(progress);
        }

        // 2. Module B: Stats Display (Now Active)
        if (statsDisplay != null)
        {
            statsDisplay.UpdateStats(currentDist, jumps, squats, timeElapsed);
        }
    }
}