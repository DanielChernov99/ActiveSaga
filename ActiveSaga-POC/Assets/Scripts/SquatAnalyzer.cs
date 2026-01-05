using UnityEngine;

public class SquatAnalyzer : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BodyTracker bodyTracker;
    [SerializeField] private HeightCalibration heightCalibration;

    [Header("Squat Settings")]
    [Tooltip("The distance (in meters) the player must lower their head to register a squat.")]
    [SerializeField] private float squatThreshold = 0.30f; // 30 cm drop required

    // Public properties to read from other scripts
    public bool IsSquatting { get; private set; } = false;
    public int SquatCounter { get; private set; } = 0;

    // Internal state to track transition from squat to standing
    private bool wasSquattingLastFrame = false;

    private void Update()
    {
        // 1. Safety Checks: Ensure everything is ready before calculating
        if (bodyTracker == null || heightCalibration == null || !heightCalibration.IsCalibrated) 
            return;

        // 2. Get Data
        float currentHeadY = bodyTracker.HeadPosition.y;
        float calibratedHeight = heightCalibration.BaseHeight;

        // 3. Squat Logic
        // Formula: If current height is lower than (Base Height - 30cm) -> We are squatting
        if (currentHeadY < (calibratedHeight - squatThreshold))
        {
            IsSquatting = true;
        }
        else
        {
            IsSquatting = false;
        }

        // 4. Counter Logic
        // If we were squatting last frame, but are NOT squatting now -> We just stood up
        if (wasSquattingLastFrame && !IsSquatting)
        {
            SquatCounter++;
            Debug.Log($"Squat Completed! Total Reps: {SquatCounter}");
        }

        // 5. Save state for the next frame
        wasSquattingLastFrame = IsSquatting;
    }
}