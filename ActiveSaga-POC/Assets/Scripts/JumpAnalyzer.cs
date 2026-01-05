using UnityEngine;
using System;

public class JumpAnalyzer : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BodyTracker bodyTracker;

    [Header("Jump Settings")]
    [Tooltip("The vertical speed (meters/second) required to trigger a jump.")]
    [SerializeField] private float jumpVelocityThreshold = 1.8f;

    [Tooltip("Time in seconds to wait before allowing another jump (prevents double jumps).")]
    [SerializeField] private float cooldownTime = 0.8f;

    // Event that other scripts can listen to
    public event Action OnJump;

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
            
            // Keep updating previous Y so we don't get a huge velocity spike when cooldown ends
            previousHeadY = bodyTracker.HeadPosition.y;
            return;
        }

        // 2. Calculate Velocity
        float currentHeadY = bodyTracker.HeadPosition.y;
        float heightChange = currentHeadY - previousHeadY;
        
        // Velocity = Distance / Time
        float currentVelocity = heightChange / Time.deltaTime;

        // 3. Check for Jump
        if (currentVelocity > jumpVelocityThreshold)
        {
            Debug.Log("Jump Detected!"); // Helps you see if it works
            
            // Notify other scripts (like PlayerLocomotion)
            OnJump?.Invoke();
            
            // Start cooldown
            currentCooldownTimer = cooldownTime;
        }

        // 4. Update for next frame
        previousHeadY = currentHeadY;
    }
}