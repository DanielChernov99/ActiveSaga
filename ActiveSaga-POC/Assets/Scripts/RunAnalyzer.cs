using UnityEngine;

public class RunAnalyzer : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BodyTracker bodyTracker;

    [Header("Detection Settings")]
    [Tooltip("Minimum vertical velocity to start detecting a run.")]
    [SerializeField] private float activationThreshold = 0.15f;

    [Tooltip("Maximum vertical velocity. If the head moves faster than this, we assume it's a jump (not a run) and ignore it.")]
    [SerializeField] private float maxActivationThreshold = 1.2f;

    [Tooltip("Smoothing speed for the run value.")]
    [SerializeField] private float smoothFactor = 5.0f;

    // Public property: 0 = Standing still, 1 = Running at full speed
    public float CurrentRunFactor { get; private set; } = 0f;

    private float previousHeadY;

    void Start()
    {
        if (bodyTracker != null)
        {
            previousHeadY = bodyTracker.HeadPosition.y;
        }
    }

    void Update()
    {
        if (bodyTracker == null) return;

        // 1. Calculate raw vertical velocity
        float currentHeadY = bodyTracker.HeadPosition.y;
        float heightChange = Mathf.Abs(currentHeadY - previousHeadY);
        float verticalVelocity = heightChange / Time.deltaTime;

        previousHeadY = currentHeadY;

        // 2. Determine target intensity
        float targetIntensity = 0f;

        // LOGIC FIX: Check that velocity is high enough to run, but LOW enough not to be a jump
        if (verticalVelocity > activationThreshold && verticalVelocity < maxActivationThreshold)
        {
            targetIntensity = 1f;
        }
        else if (verticalVelocity >= maxActivationThreshold)
        {
            // If the movement is too violent (jump), keep the previous value 
            // to prevent the run meter from dropping instantly.
            targetIntensity = CurrentRunFactor;
        }

        // 3. Smooth the value to prevent jitter
        CurrentRunFactor = Mathf.Lerp(CurrentRunFactor, targetIntensity, Time.deltaTime * smoothFactor);
    }
}