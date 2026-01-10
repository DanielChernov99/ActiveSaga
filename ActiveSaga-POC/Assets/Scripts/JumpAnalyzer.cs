using UnityEngine;
using System; // Required for Action events

public class JumpAnalyzer : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BodyTracker bodyTracker;
    
    // New: Required to know the player's standing height
    [SerializeField] private HeightCalibration heightCalibration;

    [Header("Jump Settings")]
    [Tooltip("The vertical speed (meters/second) required to trigger a jump.")]
    [SerializeField] private float jumpVelocityThreshold = 1.8f;

    [Tooltip("Time in seconds to wait before allowing another jump (prevents double jumps).")]
    [SerializeField] private float cooldownTime = 0.8f;

    [Header("Height Logic")]
    [Tooltip("If the head is lower than (BaseHeight - this value), upward movement is ignored. Prevents squats from registering as jumps.")]
    [SerializeField] private float minHeightOffsetForJump = 0.30f;

    // --- Events ---
    // Event fired when a jump is detected. 
    // Listeners: PlayerLocomotion (Physics), GameManager (Score)
    public event Action OnJump;

    // Public property for debugging in Inspector
    public int JumpCounter { get; private set; } = 0;

    private float previousHeadY;
    private float currentCooldownTimer;

    private void Start()
    {
        if (bodyTracker != null)
        {
            previousHeadY = bodyTracker.HeadPosition.y;
        }
        else
        {
            Debug.LogError("JumpAnalyzer: BodyTracker is not assigned!");
        }

        if (heightCalibration == null)
        {
            Debug.LogWarning("JumpAnalyzer: HeightCalibration is missing! The height check will be skipped.");
        }
    }

    private void Update()
    {
        if (bodyTracker == null) return;

        // 1. Handle Cooldown
        if (currentCooldownTimer > 0)
        {
            currentCooldownTimer -= Time.deltaTime;
            
            // Important: Update position during cooldown to prevent velocity spikes when cooldown ends
            previousHeadY = bodyTracker.HeadPosition.y;
            return;
        }

        // 2. Calculate Velocity
        float currentHeadY = bodyTracker.HeadPosition.y;
        float heightChange = currentHeadY - previousHeadY;
        
        // Velocity = Distance / Time
        float currentVelocity = heightChange / Time.deltaTime;

        previousHeadY = currentHeadY;

        // 3. Check for Jump
        if (currentVelocity > jumpVelocityThreshold)
        {
            // New Logic: Check if we are too low (recovering from squat)
            // If true, we ignore this "jump" because it's just standing up
            if (IsTooLowForJump(currentHeadY))
            {
                return;
            }

            // Valid Jump detected
            // Increment local counter
            JumpCounter++;
            
            // Notify listeners
            OnJump?.Invoke();
            
            // Start cooldown
            currentCooldownTimer = cooldownTime;
        }
    }

    // Helper function to check height relative to calibration
    private bool IsTooLowForJump(float currentY)
    {
        // If calibration is missing or not ready, allow the jump by default
        if (heightCalibration == null || !heightCalibration.IsCalibrated) return false;

        float minAllowedY = heightCalibration.BaseHeight - minHeightOffsetForJump;

        // If current height is below the threshold, it is considered a squat recovery
        return currentY < minAllowedY;
    }
}