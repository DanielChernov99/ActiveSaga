using UnityEngine;
using TMPro;
using ActiveSaga.BossFight.Core;
using ActiveSaga.BossFight.Entities;

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

    private void Start()
    {
        if (gameManager != null)
        {
            gameManager.OnStatsUpdated += HandleGameUpdate;

            if (statsDisplay != null)
            {
                statsDisplay.SetGoals(
                    gameManager.levelTargetDistance, 
                    gameManager.goalJumps, 
                    gameManager.goalSquats
                );
            }

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

    // ---------------- WAVES ----------------

    private void OnWaveStarted(WaveStartedEvent e)
    {
        if (waveText != null)
        {
            waveText.text = $"Wave: {e.waveIndex}";
        }
    }

    // ---------------- RUN STATS ----------------

    private void HandleGameUpdate(float currentDist, int jumps, int squats, float timeElapsed)
    {
        if (progressBar != null)
        {
            float maxDist = gameManager.levelTargetDistance;
            float progress = (maxDist > 0) ? (currentDist / maxDist) : 0;
            progressBar.UpdateVisuals(progress);
        }

        if (statsDisplay != null)
        {
            statsDisplay.UpdateStats(currentDist, jumps, squats, timeElapsed);
        }
    }
}