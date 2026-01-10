using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    [Header("Core References")]
    [Tooltip("The XR Rig or parent object to move.")]
    [SerializeField] private Transform playerRoot;
    
    [Tooltip("Usually the Main Camera - determines movement direction.")]
    [SerializeField] private Transform forwardReference;

    [Header("Analyzers")]
    [SerializeField] private RunAnalyzer runAnalyzer;
    [SerializeField] private SquatAnalyzer squatAnalyzer;
    [SerializeField] private JumpAnalyzer jumpAnalyzer;

    [Header("Movement Settings")]
    [SerializeField] private float runSpeed = 8.0f;
    [SerializeField] private float squatSpeed = 1.5f;
    [Tooltip("How fast the player accelerates/decelerates. Higher = more responsive.")]
    [SerializeField] private float momentumResponsiveness = 5.0f;

    [Header("Jump Physics")]
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float gravity = 9.81f;
    [Tooltip("Minimum run intensity required to perform a running jump. Below this, it's a stationary jump.")]
    [SerializeField] private float minRunForMomentumJump = 0.2f;
    
    [Header("Environment")]
    [Tooltip("The Y position of the floor.")]
    [SerializeField] private float groundLevel = 0f;

    // --- Private State Variables ---
    private float verticalVelocity;
    private bool isGrounded = true;
    
    // Tracks the current smoothed speed for momentum
    private float currentSpeed = 0f; 
    
    // If true, the player jumped from a standstill and cannot move horizontally in mid-air
    private bool isStationaryJumpLock = false;

    // Cached data from Events
    private float currentInputRunIntensity = 0f;
    private bool isSquatting = false;

    private void OnEnable()
    {
        // Subscribe to all Locomotion Events
        if (jumpAnalyzer != null) jumpAnalyzer.OnJump += HandleJump;
        if (runAnalyzer != null) runAnalyzer.OnRunIntensity += HandleRunInput;
        if (squatAnalyzer != null) squatAnalyzer.OnSquatStateChanged += HandleSquatState;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent errors
        if (jumpAnalyzer != null) jumpAnalyzer.OnJump -= HandleJump;
        if (runAnalyzer != null) runAnalyzer.OnRunIntensity -= HandleRunInput;
        if (squatAnalyzer != null) squatAnalyzer.OnSquatStateChanged -= HandleSquatState;
    }

    private void Update()
    {
        if (playerRoot == null || forwardReference == null) return;

        MoveHorizontal();
        ApplyGravity();
    }

    // Event Listener for Run
    private void HandleRunInput(float intensity)
    {
        currentInputRunIntensity = intensity;
    }

    // Event Listener for Squat
    private void HandleSquatState(bool state)
    {
        isSquatting = state;
    }

    private void MoveHorizontal()
    {
        // 1. Determine Target Speed based on cached event data
        float targetSpeed = 0f;

        if (isSquatting)
        {
            // Squatting logic: Fixed slow speed
            // If squatting, we require a tiny bit of movement to start moving
            if (currentInputRunIntensity > 0.1f) 
            {
                targetSpeed = squatSpeed;
            }
            else 
            {
                targetSpeed = 0f;
            }
        }
        else
        {
            // Normal Running logic
            targetSpeed = runSpeed * currentInputRunIntensity;
        }

        // 2. Handle Stationary Jump Locking
        // If we are in the air AND locked, force target speed to 0 (prevent drifting)
        if (!isGrounded && isStationaryJumpLock)
        {
            targetSpeed = 0f;
        }

        // 3. Apply Momentum (Smoothing)
        // Lerp current speed towards target speed for smooth transitions
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * momentumResponsiveness);

        // 4. Move Player
        // Only apply translation if speed is significant
        if (currentSpeed > 0.01f)
        {
            Vector3 forwardDir = forwardReference.forward;
            forwardDir.y = 0; // Flatten direction so we don't fly up/down
            forwardDir.Normalize();

            playerRoot.Translate(forwardDir * currentSpeed * Time.deltaTime, Space.World);
        }
    }

    private void HandleJump()
    {
        // Block jump if in air or if squatting
        if (!isGrounded) return;
        if (isSquatting) return;

        // Determine if this is a Running Jump or a Standing Jump based on current intensity
        if (currentInputRunIntensity > minRunForMomentumJump)
        {
            // Running Jump: Allow momentum to continue in air
            isStationaryJumpLock = false;
            Debug.Log("Action: Running Jump!");
        }
        else
        {
            // Standing Jump: Lock movement so player goes straight up/down
            isStationaryJumpLock = true;
            currentSpeed = 0f; // Kill existing momentum immediately
            Debug.Log("Action: Standing Jump (Locked)");
        }

        verticalVelocity = jumpForce;
        isGrounded = false;
    }

    private void ApplyGravity()
    {
        // Apply gravity
        verticalVelocity -= gravity * Time.deltaTime;
        playerRoot.Translate(Vector3.up * verticalVelocity * Time.deltaTime, Space.World);

        // Ground Check
        if (playerRoot.position.y <= groundLevel)
        {
            // Snap to floor
            Vector3 pos = playerRoot.position;
            pos.y = groundLevel;
            playerRoot.position = pos;

            isGrounded = true;
            verticalVelocity = 0;
            
            // Unlock the jump restriction when landing
            isStationaryJumpLock = false;
        }
    }
}