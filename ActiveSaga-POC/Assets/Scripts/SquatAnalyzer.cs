using UnityEngine;
using System; // Required for Action events

public class SquatAnalyzer : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BodyTracker bodyTracker;
    [SerializeField] private HeightCalibration heightCalibration;

    [Header("Squat Settings")]
    [Tooltip("The distance (in meters) the player must lower their head to register a squat.")]
    [SerializeField] private float squatThreshold = 0.30f; 

    // --- Events ---
    // Event fired when entering or exiting the squat posture (true = down, false = up)
    // Useful for PlayerLocomotion to disable movement while squatting.
    public event Action<bool> OnSquatStateChanged;

    // Event fired only when a full squat rep is completed (standing back up)
    // Useful for GameManager to count score.
    public event Action OnSquatCompleted;

    // Public properties
    public bool IsSquatting { get; private set; } = false;
    
    // We keep a local counter for debugging in the Inspector, 
    // even though GameManager will do the actual game scoring.
    public int SquatCounter { get; private set; } = 0;

    private bool wasSquattingLastFrame = false;

    private void Update()
    {
        // 1. Safety Checks
        if (bodyTracker == null || heightCalibration == null || !heightCalibration.IsCalibrated) 
            return;

        // 2. Get Data
        float currentHeadY = bodyTracker.HeadPosition.y;
        float calibratedHeight = heightCalibration.BaseHeight;

        // 3. Determine current state
        // Check if head is below the threshold
        bool isCurrentlyDown = currentHeadY < (calibratedHeight - squatThreshold);

        // 4. Handle State Changes (Entering or Exiting Squat)
        if (isCurrentlyDown != IsSquatting)
        {
            IsSquatting = isCurrentlyDown;
            // Notify listeners (Locomotion) that state changed
            OnSquatStateChanged?.Invoke(IsSquatting);
        }

        // 5. Handle Rep Completion (Rising up from a squat)
        // If we were down last frame, but we are up now -> Rep complete
        if (wasSquattingLastFrame && !isCurrentlyDown)
        {
            SquatCounter++;
            // Notify listeners (GameManager) that a rep is done
            OnSquatCompleted?.Invoke();
        }

        // 6. Save state for the next frame
        wasSquattingLastFrame = isCurrentlyDown;
    }
}