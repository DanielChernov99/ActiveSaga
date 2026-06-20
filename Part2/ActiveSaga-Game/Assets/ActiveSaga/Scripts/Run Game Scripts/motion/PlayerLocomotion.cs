using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerLocomotion : MonoBehaviour
{
    [Header("Core References")]
    [Tooltip("Reference to the camera or head to determine forward direction")]
    [SerializeField] private Transform forwardReference;

    [Header("Game State")]
    [SerializeField] private GameManager gameManager;

    [Header("Analyzers Events")]
    [SerializeField] private RunAnalyzer runAnalyzer;
    [SerializeField] private SquatAnalyzer squatAnalyzer;
    [SerializeField] private JumpAnalyzer jumpAnalyzer;

    [Header("Movement Settings")]
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float squatSpeed = 1.5f;

    [Tooltip("How fast the player reaches max speed")]
    [SerializeField] private float acceleration = 6f;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = 20f;

    private CharacterController characterController;

    private float currentSpeed;
    private float verticalVelocity;
    private float runIntensity;
    private bool isSquatting;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (forwardReference == null)
        {
            Debug.LogError("PlayerLocomotion: Forward Reference is missing! Assign the camera/head.");
            enabled = false;
            return;
        }

        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
    }

    private void OnEnable()
    {
        if (runAnalyzer != null)
        {
            runAnalyzer.OnRunIntensity += HandleRun;
        }

        if (squatAnalyzer != null)
        {
            squatAnalyzer.OnSquatStateChanged += HandleSquat;
        }

        if (jumpAnalyzer != null)
        {
            jumpAnalyzer.OnJump += HandleJump;
        }
    }

    private void OnDisable()
    {
        if (runAnalyzer != null)
        {
            runAnalyzer.OnRunIntensity -= HandleRun;
        }

        if (squatAnalyzer != null)
        {
            squatAnalyzer.OnSquatStateChanged -= HandleSquat;
        }

        if (jumpAnalyzer != null)
        {
            jumpAnalyzer.OnJump -= HandleJump;
        }
    }

    private void Update()
    {
        if (!IsGameplayActive())
        {
            StopMovement();
            return;
        }

        UpdateSpeed();
        UpdateVerticalVelocity();
        Move();
    }

    private void HandleRun(float intensity)
    {
        if (!IsGameplayActive())
        {
            runIntensity = 0f;
            currentSpeed = 0f;
            return;
        }

        runIntensity = intensity;
    }

    private void HandleSquat(bool state)
    {
        if (!IsGameplayActive())
        {
            isSquatting = false;
            return;
        }

        isSquatting = state;
    }

    private void HandleJump()
    {
        if (!IsGameplayActive())
        {
            return;
        }

        if (gameManager != null && gameManager.IsPlayerStunned())
        {
            return;
        }

        if (characterController.isGrounded && !isSquatting)
        {
            verticalVelocity = jumpForce;
        }
    }

    private bool IsGameplayActive()
    {
        return gameManager == null || gameManager.IsGameActive;
    }

    private void StopMovement()
    {
        currentSpeed = 0f;
        runIntensity = 0f;
        verticalVelocity = 0f;
        isSquatting = false;
    }

    private void UpdateSpeed()
    {
        bool isStunned = gameManager != null && gameManager.IsPlayerStunned();

        if (isStunned)
        {
            currentSpeed = 0f;
            runIntensity = 0f;
            return;
        }

        float targetSpeed = 0f;

        if (isSquatting)
        {
            targetSpeed = runIntensity > 0.1f ? squatSpeed : 0f;
        }
        else
        {
            targetSpeed = runSpeed * runIntensity;
        }

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
            if (verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
    }

    private void Move()
    {
        Vector3 forward = Vector3.ProjectOnPlane(
            forwardReference.forward,
            Vector3.up
        ).normalized;

        Vector3 velocity =
            forward * currentSpeed +
            Vector3.up * verticalVelocity;

        characterController.Move(velocity * Time.deltaTime);
    }
}