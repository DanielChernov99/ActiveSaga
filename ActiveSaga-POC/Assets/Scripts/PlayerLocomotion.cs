using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Transform playerTransform; // The XR Rig
    [SerializeField] private RunAnalyzer runAnalyzer;   // Connect to the brain
    
    [Header("Path Settings")]
    [Tooltip("Drag an object here to define the forward direction (e.g. an Empty GameObject arrow)")]
    [SerializeField] private Transform pathDirectionReference; 

    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 4.0f;

    void Update()
    {
        if (runAnalyzer == null || playerTransform == null || pathDirectionReference == null)
        {
            return;
        }

        // Get the smoothed intensity from the analyzer (0 to 1)
        float intensity = runAnalyzer.CurrentRunFactor;

        // Only move if there is significant intensity
        if (intensity > 0.01f)
        {
            // CHANGE: Use the fixed path direction instead of the head direction
            Vector3 moveDirection = pathDirectionReference.forward;
            
            // Ensure we stay flat on the ground
            moveDirection.y = 0; 
            moveDirection.Normalize();

            // Calculate actual speed
            float currentSpeed = maxSpeed * intensity;

            // Move
            playerTransform.Translate(moveDirection * currentSpeed * Time.deltaTime, Space.World);
        }
    }
}