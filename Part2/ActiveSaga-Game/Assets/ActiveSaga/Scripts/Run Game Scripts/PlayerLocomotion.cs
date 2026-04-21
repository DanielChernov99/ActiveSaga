using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerLocomotion : MonoBehaviour
{
    [Header("Core References")]
    [Tooltip("Reference to the camera or head to determine forward direction")]
    [SerializeField] private Transform forwardReference;
    
    // Auto-fetched in Awake
    private CharacterController characterController;

    [Header("Analyzers Events")]
    [SerializeField] private RunAnalyzer runAnalyzer;
    [SerializeField] private SquatAnalyzer squatAnalyzer;
    [SerializeField] private JumpAnalyzer jumpAnalyzer;

    [Header("Movement Settings")]
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float squatSpeed = 1.5f;
    [Tooltip("How fast the player reaches max speed (Smoothing)")]
    [SerializeField] private float acceleration = 6f;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpForce = 5f;
    [Tooltip("Higher gravity feels less 'floaty' in games. 20 is a good value.")]
    [SerializeField] private float gravity = 20f;

    // Internal State
    private float currentSpeed;
    private float verticalVelocity;
    private float runIntensity;
    private bool isSquatting;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        
        // Safety check
        if (forwardReference == null)
        {
            Debug.LogError("PlayerLocomotion: Forward Reference is missing! Assign the Camera.");
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (runAnalyzer) runAnalyzer.OnRunIntensity += HandleRun;
        if (squatAnalyzer) squatAnalyzer.OnSquatStateChanged += HandleSquat;
        if (jumpAnalyzer) jumpAnalyzer.OnJump += HandleJump;
    }

    private void OnDisable()
    {
        if (runAnalyzer) runAnalyzer.OnRunIntensity -= HandleRun;
        if (squatAnalyzer) squatAnalyzer.OnSquatStateChanged -= HandleSquat;
        if (jumpAnalyzer) jumpAnalyzer.OnJump -= HandleJump;
    }

    private void Update()
    {
        UpdateSpeed();
        UpdateVerticalVelocity();
        Move();
    }

    // --- Event Handlers ---

    private void HandleRun(float intensity) => runIntensity = intensity;
    private void HandleSquat(bool state) => isSquatting = state;

    private void HandleJump()
    {
        // Only jump if grounded and not squatting
        if (characterController.isGrounded && !isSquatting)
        {
            verticalVelocity = jumpForce;
        }
    }

    // --- Physics Logic ---

    private void UpdateSpeed()
    {
        float targetSpeed = 0f;

        if (isSquatting)
        {
            // Move slowly if squatting and trying to run
            targetSpeed = runIntensity > 0.1f ? squatSpeed : 0f;
        }
        else
        {
            targetSpeed = runSpeed * runIntensity;
        }

        // Smoothly interpolate current speed towards target speed
        currentSpeed = Mathf.Lerp(
            currentSpeed,
            targetSpeed,
            Time.deltaTime * acceleration
        );
    }

    private void UpdateVerticalVelocity()
    {
        if (characterController.isGrounded)
        {
            // Apply small downward force to stick to ground (prevents jitter)
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;
        }
        else
        {
            // Apply Gravity
            verticalVelocity -= gravity * Time.deltaTime;
        }
    }

    private void Move()
    {
        // 1. Get Forward Direction (Flattened on Y axis)
        // This ensures looking down/up doesn't affect speed
        Vector3 forward = Vector3.ProjectOnPlane(forwardReference.forward, Vector3.up).normalized;

        // 2. Combine Forward Speed + Vertical Velocity (Gravity/Jump)
        Vector3 velocity = (forward * currentSpeed) + (Vector3.up * verticalVelocity);

        // 3. Move the Character Controller
        characterController.Move(velocity * Time.deltaTime);
    }
}