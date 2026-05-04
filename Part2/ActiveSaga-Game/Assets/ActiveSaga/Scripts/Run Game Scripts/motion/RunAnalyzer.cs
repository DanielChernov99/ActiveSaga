using UnityEngine;
using System; // Required for Actions/Events

public class RunAnalyzer : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BodyTracker bodyTracker;

    [Header("Run Settings")]
    [Tooltip("How much head bobbing translates to speed. Higher = easier to run.")]
    [SerializeField] private float sensitivity = 0.8f; 
    
    [Tooltip("Smoothing speed. Higher = more responsive, Lower = smoother.")]
    [SerializeField] private float smoothFactor = 5.0f;

    [Header("Thresholds")]
    [Tooltip("Minimum vertical speed to count as movement (filters breathing).")]
    [SerializeField] private float minVelocity = 0.15f;

    [Tooltip("Maximum vertical speed. Anything faster is considered a Jump, not a Run.")]
    [SerializeField] private float maxJumpVelocity = 1.2f;

    // Event invoked every frame with the current run intensity (0 to 1)
    // This allows other scripts (Locomotion, GameManager) to react without checking this script constantly.
    public event Action<float> OnRunIntensity;

    // Public property for debugging or legacy support
    public float RunFactor { get; private set; } = 0f;

    private float previousHeadY;

    private void Start()
    {
        if (bodyTracker != null)
        {
            previousHeadY = bodyTracker.HeadPosition.y;
        }
    }

    private void Update()
    {
        if (bodyTracker == null) return;

        // 1. Calculate Vertical Velocity (Meters per Second)
        float currentHeadY = bodyTracker.HeadPosition.y;
        float verticalDelta = Mathf.Abs(currentHeadY - previousHeadY);
        float velocity = verticalDelta / Time.deltaTime;

        previousHeadY = currentHeadY;

        // 2. Determine Target Run Intensity
        float targetRunFactor = 0f;

        // A. If velocity is too high (Jump) -> Keep current speed (don't stop abruptly)
        if (velocity >= maxJumpVelocity)
        {
            targetRunFactor = RunFactor; 
        }
        // B. If velocity is too low (Breathing/Standing) -> Stop
        else if (velocity < minVelocity)
        {
            targetRunFactor = 0f;
        }
        // C. Normal Running -> Calculate intensity based on speed
        else
        {
            targetRunFactor = Mathf.Clamp01(velocity * sensitivity);
        }

        // 3. Smooth the output
        RunFactor = Mathf.Lerp(RunFactor, targetRunFactor, Time.deltaTime * smoothFactor);

        // 4. Fire the Event
        // We broadcast the data if there is movement, or if we just stopped.
        if (RunFactor > 0.01f)
        {
            OnRunIntensity?.Invoke(RunFactor);
        }
        else if (RunFactor < 0.01f && RunFactor > 0)
        {
            // Send a hard 0 to ensure the player stops completely
            OnRunIntensity?.Invoke(0f);
        }
    }
}