using UnityEngine;
using System; // Required for Action events

public class SquatAnalyzer : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BodyTracker bodyTracker;
    [SerializeField] private HeightCalibration heightCalibration;

    [Header("Squat Settings")]
    [Tooltip("Distance below base height to TRIGGER a squat (e.g., 0.30m down).")]
    [SerializeField] private float squatDownThreshold = 0.30f; 

    [Tooltip("Distance below base height to RESET a squat (e.g., 0.10m down). Must be smaller than Down Threshold!")]
    [SerializeField] private float squatUpThreshold = 0.10f;

    // --- Events ---
    // Fired when entering/exiting squat state (true = down, false = up)
    public event Action<bool> OnSquatStateChanged;

    // Fired only when a full repetition is completed (standing back up)
    public event Action OnSquatCompleted;

    // Public properties
    public bool IsSquatting { get; private set; } = false;
    public int SquatCounter { get; private set; } = 0;

    private void Update()
    {
        // 1. Safety Checks
        if (bodyTracker == null || heightCalibration == null || !heightCalibration.IsCalibrated) 
            return;

        // 2. Get Data
        float currentHeadY = bodyTracker.HeadPosition.y;
        float baseHeight = heightCalibration.BaseHeight;

        // Calculate exact world Y positions for thresholds
        float thresholdDownY = baseHeight - squatDownThreshold; // Lower line (Entry)
        float thresholdUpY = baseHeight - squatUpThreshold;     // Higher line (Exit)

        // 3. Logic - State Machine with Hysteresis
        if (!IsSquatting)
        {
            // Standing state: Check if we went DOWN enough to enter squat
            if (currentHeadY < thresholdDownY)
            {
                IsSquatting = true;
                OnSquatStateChanged?.Invoke(true); // Notify Locomotion to stop/slow down
            }
        }
        else
        {
            // Squatting state: Check if we went UP enough to exit squat
            if (currentHeadY > thresholdUpY)
            {
                IsSquatting = false;
                SquatCounter++; // Count the rep only on the way up
                
                OnSquatStateChanged?.Invoke(false); // Release Locomotion
                OnSquatCompleted?.Invoke(); // Notify GameManager for score/distance
            }
        }
    }
}