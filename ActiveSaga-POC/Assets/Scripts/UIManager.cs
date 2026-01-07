using UnityEngine;
using TMPro; // If you use TextMeshPro

public class UIManager : MonoBehaviour
{
    [Header("Game Logic Reference")]
    [SerializeField] private GameManager gameManager;

    [Header("UI Components")]
    [SerializeField] private RunningProgressBar progressBar;
    [SerializeField] private TextMeshProUGUI statsText; // Optional: To show exact numbers

    private void Start()
    {
        if (gameManager != null)
        {
            // Subscribe to the event existing in your GameManager
            gameManager.OnStatsUpdated += UpdateUI;
            
            // Initialize UI immediately
            UpdateUI(gameManager.CurrentDistance, gameManager.TotalJumps, gameManager.TotalSquats);
        }
    }

    private void OnDestroy()
    {
        // IMPORTANT: Unsubscribe to prevent memory leaks
        if (gameManager != null)
        {
            gameManager.OnStatsUpdated -= UpdateUI;
        }
    }

    // This function matches the signature of your Action<float, int, int>
    private void UpdateUI(float currentDist, int jumps, int squats)
    {
        // 1. Calculate Progress Percentage for the bar
        // We get the target distance directly from your public variable in GameManager
        float maxDist = gameManager.levelTargetDistance;
        
        // Avoid division by zero
        float progress = (maxDist > 0) ? (currentDist / maxDist) : 0;
        
        // Clamp between 0 and 1 just in case
        progress = Mathf.Clamp01(progress);

        // 2. Update the Bar
        if (progressBar != null)
        {
            progressBar.UpdateVisuals(progress);
        }

        // 3. Optional: Update Text display
        if (statsText != null)
        {
            statsText.text = $"Dist: {currentDist:F1}m | Jumps: {jumps} | Squats: {squats}";
        }
    }
}