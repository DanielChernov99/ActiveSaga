using UnityEngine;

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

    // 0 = Standing, 0.5 = Jogging, 1.0 = Sprints
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

        // LOGIC:
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
            // Convert velocity to a 0-1 factor based on sensitivity
            targetRunFactor = Mathf.Clamp01(velocity * sensitivity);
        }

        // 3. Smooth the output
        RunFactor = Mathf.Lerp(RunFactor, targetRunFactor, Time.deltaTime * smoothFactor);
    }
}