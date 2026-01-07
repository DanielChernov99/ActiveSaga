using UnityEngine;
using System; // Required for Action events

public class JumpAnalyzer : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BodyTracker bodyTracker;

    [Header("Jump Settings")]
    [Tooltip("The vertical speed (meters/second) required to trigger a jump.")]
    [SerializeField] private float jumpVelocityThreshold = 1.8f;

    [Tooltip("Time in seconds to wait before allowing another jump (prevents double jumps).")]
    [SerializeField] private float cooldownTime = 0.8f;

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
            // Debug Log (Optional)
            // Debug.Log("Jump Detected!"); 

            // Increment local counter
            JumpCounter++;
            
            // Notify listeners
            OnJump?.Invoke();
            
            // Start cooldown
            currentCooldownTimer = cooldownTime;
        }
    }
}