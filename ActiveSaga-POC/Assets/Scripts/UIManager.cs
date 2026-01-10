using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Game Manager Connection")]
    [SerializeField] private GameManager gameManager;

    [Header("Modules")]
    [Tooltip("The Progress Bar Logic")]
    [SerializeField] private RunningProgressBar progressBar;
    
    // Future: [SerializeField] private StatsDisplay statsDisplay; 

    private void Start()
    {
        if (gameManager != null)
        {
            gameManager.OnStatsUpdated += HandleGameUpdate;
            
            // Initial reset
            HandleGameUpdate(0, 0, 0);
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

    // This function receives ALL data, but currently only uses what is needed for the bar.
    // In the future, you simply pass the other variables (jumps, squats) to the Stats module here.
    private void HandleGameUpdate(float currentDist, int jumps, int squats)
    {
        // 1. Module A: Progress Bar
        if (progressBar != null)
        {
            float maxDist = gameManager.levelTargetDistance;
            
            // Protect against division by zero
            float progress = (maxDist > 0) ? (currentDist / maxDist) : 0;
            
            progressBar.UpdateVisuals(progress);
        }

        // 2. Module B: Stats (Future Implementation)
        // if (statsDisplay != null)
        // {
        //     statsDisplay.UpdateStats(currentDist, jumps, squats);
        // }
    }
}