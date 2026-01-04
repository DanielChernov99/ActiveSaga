using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Transform playerTransform; // The XR Rig
    [SerializeField] private Transform headTransform;   // For direction
    [SerializeField] private RunAnalyzer runAnalyzer;   // Connect to the brain

    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 4.0f;

    void Update()
    {
        if (runAnalyzer == null || playerTransform == null || headTransform == null) return;

        // Get the smoothed intensity from the analyzer (0 to 1)
        float intensity = runAnalyzer.CurrentRunFactor;

        // Only move if there is significant intensity
        if (intensity > 0.01f)
        {
            // Determine direction based on where the player is looking
            Vector3 moveDirection = headTransform.forward;
            moveDirection.y = 0; // Keep on ground
            moveDirection.Normalize();

            // Calculate actual speed
            float currentSpeed = maxSpeed * intensity;

            // Move
            playerTransform.Translate(moveDirection * currentSpeed * Time.deltaTime, Space.World);
        }
    }
}