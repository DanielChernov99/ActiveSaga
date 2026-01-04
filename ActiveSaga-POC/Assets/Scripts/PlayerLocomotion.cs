using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private RunAnalyzer runAnalyzer;
    [SerializeField] private JumpAnalyzer jumpAnalyzer;
    
    [Header("Path Settings")]
    [Tooltip("Drag an object here to define the forward direction")]
    [SerializeField] private Transform pathDirectionReference; 

    [Header("Movement Settings")]
    [SerializeField] private float maxRunSpeed = 4.0f;
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float gravity = 9.81f;

    [Header("Air Control")]
    [Tooltip("Multiplier for speed while in air. Lower this to jump shorter distances.")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float airSpeedFactor = 0.5f; 

    [Tooltip("If speed is below this value when jumping, the jump will be vertical (0 forward speed).")]
    [SerializeField] private float minMomentumToMaintain = 1.0f;
    
    private float verticalVelocity = 0f;
    private bool isGrounded = true;
    
    // Store the speed we had just before leaving the ground
    private float momentumSpeed = 0f;

    private void OnEnable()
    {
        if (jumpAnalyzer != null)
        {
            jumpAnalyzer.OnJumpDetected += PerformJump;
        }
    }

    private void OnDisable()
    {
        if (jumpAnalyzer != null)
        {
            jumpAnalyzer.OnJumpDetected -= PerformJump;
        }
    }

    void Update()
    {
        if (runAnalyzer == null || playerTransform == null || pathDirectionReference == null)
        {
            return;
        }

        // --- 1. Calculate Horizontal Speed ---
        float targetSpeed = 0f;

        if (isGrounded)
        {
            // On Ground: Speed is determined by how fast the player runs
            float runIntensity = runAnalyzer.CurrentRunFactor;
            
            if (runIntensity > 0.05f) 
            {
                targetSpeed = maxRunSpeed * runIntensity;
            }
            
            // constantly update momentum while on ground
            momentumSpeed = targetSpeed;
        }
        else
        {
            // In Air: Use momentum calculated at takeoff
            targetSpeed = momentumSpeed * airSpeedFactor;
        }

        // Apply Horizontal Movement
        Vector3 forwardDir = pathDirectionReference.forward;
        forwardDir.y = 0; 
        forwardDir.Normalize();
        Vector3 horizontalMove = forwardDir * targetSpeed * Time.deltaTime;

        // --- 2. Calculate Vertical Movement (Gravity) ---
        ApplyGravity();
        Vector3 verticalMove = Vector3.up * verticalVelocity * Time.deltaTime;

        // --- 3. Apply Total Movement ---
        playerTransform.Translate(horizontalMove + verticalMove, Space.World);

        // --- 4. Ground Check ---
        if (playerTransform.position.y <= 0)
        {
            Vector3 pos = playerTransform.position;
            pos.y = 0;
            playerTransform.position = pos;
            
            isGrounded = true;
            verticalVelocity = 0;
        }
    }

    private void PerformJump()
    {
        if (isGrounded)
        {
            // LOGIC FIX:
            // If we are moving very slowly (likely standing still), kill the momentum entirely.
            if (momentumSpeed < minMomentumToMaintain)
            {
                momentumSpeed = 0f;
                Debug.Log("Standing Jump (Vertical Only)");
            }
            else
            {
                Debug.Log("Running Jump (Forward Momentum)");
            }

            verticalVelocity = jumpForce;
            isGrounded = false;
        }
    }

    private void ApplyGravity()
    {
        if (!isGrounded || playerTransform.position.y > 0)
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
    }
}