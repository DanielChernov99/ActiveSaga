using UnityEngine;

public class SquatAnalyzer : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BodyTracker bodyTracker;
    [SerializeField] private HeightCalibration heightCalibration;

    [Header("Squat Settings")]
    [Tooltip("The distance (in meters) the player must lower their head to register a squat.")]
    [SerializeField] private float squatThreshold = 0.30f; // 30 cm

    // Public properties to read from other scripts
    public bool IsSquatting { get; private set; } = false;
    public int SquatCounter { get; private set; } = 0;

    private bool wasSquattingLastFrame = false;

    void Update()
    {
        // Ensure calibration is complete before checking logic
        if (bodyTracker == null || heightCalibration == null || !heightCalibration.IsCalibrated) 
            return;

        float currentHeadY = bodyTracker.HeadPosition.y;
        float calibratedHeight = heightCalibration.BaseHeight;

        // Check if the player's head is below the threshold
        // Example: Base 1.80m - Threshold 0.30m = 1.50m. If head is < 1.50m, it's a squat.
        if (currentHeadY < (calibratedHeight - squatThreshold))
        {
            IsSquatting = true;
        }
        else
        {
            IsSquatting = false;
        }

        // Count logic: Increment only when the player stands UP (finishes the squat)
        if (wasSquattingLastFrame && !IsSquatting)
        {
            SquatCounter++;
            Debug.Log($"Squat Completed! Total Count: {SquatCounter}");
        }

        // Save state for the next frame
        wasSquattingLastFrame = IsSquatting;
    }
}