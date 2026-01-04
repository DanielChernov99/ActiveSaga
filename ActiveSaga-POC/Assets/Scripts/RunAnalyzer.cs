using UnityEngine;

public class RunAnalyzer : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BodyTracker bodyTracker;

    [Header("Detection Settings")]
    [SerializeField] private float activationThreshold = 0.15f;
    [SerializeField] private float smoothFactor = 5.0f;

    // Public property that other scripts can read
    // 0 = Standing still, 1 = Running at full speed
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

        // 1. Calculate raw velocity
        float currentHeadY = bodyTracker.HeadPosition.y;
        float heightChange = Mathf.Abs(currentHeadY - previousHeadY);
        float verticalVelocity = heightChange / Time.deltaTime;

        previousHeadY = currentHeadY;

        // 2. Determine target intensity (0 or 1)
        float targetIntensity = 0f;
        if (verticalVelocity > activationThreshold)
        {
            targetIntensity = 1f;
        }

        // 3. Smooth the value so it doesn't jitter
        CurrentRunFactor = Mathf.Lerp(CurrentRunFactor, targetIntensity, Time.deltaTime * smoothFactor);
    }
}