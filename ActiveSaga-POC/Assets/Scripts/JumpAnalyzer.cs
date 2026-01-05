using UnityEngine;
using System;

public class JumpAnalyzer : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BodyTracker bodyTracker;

    [Header("Detection Settings")]
    [Tooltip("How fast the head must move up to trigger a jump (Meters/Second).")]
    [SerializeField] private float jumpVelocityThreshold = 1.5f;

    [Tooltip("Time in seconds to wait before detecting the next jump.")]
    [SerializeField] private float jumpCooldown = 0.8f;

    // Event that other scripts can listen to
    public event Action OnJumpDetected;

    private float previousHeadY;
    private float cooldownTimer = 0f;

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

        // 1. Manage Cooldown
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            // Update previous position even during cooldown to prevent spikes when cooldown ends
            previousHeadY = bodyTracker.HeadPosition.y; 
            return;
        }

        // 2. Calculate Vertical Velocity
        float currentHeadY = bodyTracker.HeadPosition.y;
        float verticalVelocity = (currentHeadY - previousHeadY) / Time.deltaTime;

        // 3. Check for Jump
        // We only care if velocity is POSITIVE (going up) and above threshold
        if (verticalVelocity > jumpVelocityThreshold)
        {
            FireJump();
        }

        previousHeadY = currentHeadY;
    }

    private void FireJump()
    {
        Debug.Log("Jump Detected!");
        OnJumpDetected?.Invoke(); // Notify listeners
        cooldownTimer = jumpCooldown; // Start cooldown
    }
}