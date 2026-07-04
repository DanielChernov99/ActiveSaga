using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class VRPlayerHitboxFollower : MonoBehaviour
{
    [Header("Analyzer References")]
    [SerializeField] private JumpAnalyzer jumpAnalyzer;
    [SerializeField] private SquatAnalyzer squatAnalyzer;

    [Header("Base Collider Position")]
    [Tooltip("Local Y position of the capsule center while standing.")]
    [SerializeField] private float standingCenterY = 0.95f;

    [Header("Jump Collider Movement")]
    [Tooltip("How much the hitbox collider moves up when a jump is detected.")]
    [SerializeField] private float jumpColliderOffset = 1.0f;

    [Tooltip("How long the hitbox stays in jump motion.")]
    [SerializeField] private float jumpDuration = 0.65f;

    [Header("Squat Collider Movement")]
    [Tooltip("How much the hitbox collider moves down while squatting. Use a positive value.")]
    [SerializeField] private float squatColliderOffset = 0.75f;

    [Tooltip("How fast the hitbox moves toward its squat/stand position.")]
    [SerializeField] private float squatMoveSpeed = 12f;

    private CapsuleCollider capsule;

    private Vector3 originalCenter;
    private float originalHeight;
    private float originalRadius;

    private float currentSquatOffset;
    private float targetSquatOffset;

    private bool isJumping;
    private float jumpTimer;

    private void Awake()
    {
        capsule = GetComponent<CapsuleCollider>();

        originalCenter = capsule.center;
        originalHeight = capsule.height;
        originalRadius = capsule.radius;
    }

    private void OnEnable()
    {
        if (jumpAnalyzer != null)
        {
            jumpAnalyzer.OnJump += HandleJump;
        }

        if (squatAnalyzer != null)
        {
            squatAnalyzer.OnSquatStateChanged += HandleSquatStateChanged;
        }
    }

    private void OnDisable()
    {
        if (jumpAnalyzer != null)
        {
            jumpAnalyzer.OnJump -= HandleJump;
        }

        if (squatAnalyzer != null)
        {
            squatAnalyzer.OnSquatStateChanged -= HandleSquatStateChanged;
        }
    }

    private void LateUpdate()
    {
        UpdateSquatOffset();
        UpdateColliderPosition();
    }

    private void HandleJump()
    {
        isJumping = true;
        jumpTimer = 0f;
    }

    private void HandleSquatStateChanged(bool isSquatting)
    {
        targetSquatOffset = isSquatting
            ? -Mathf.Abs(squatColliderOffset)
            : 0f;
    }

    private void UpdateSquatOffset()
    {
        currentSquatOffset = Mathf.Lerp(
            currentSquatOffset,
            targetSquatOffset,
            Time.deltaTime * squatMoveSpeed
        );

        if (Mathf.Abs(currentSquatOffset - targetSquatOffset) < 0.01f)
        {
            currentSquatOffset = targetSquatOffset;
        }
    }

    private void UpdateColliderPosition()
    {
        float jumpOffset = GetJumpOffset();

        capsule.height = originalHeight;
        capsule.radius = originalRadius;

        Vector3 nextCenter = originalCenter;

        // Standing base height + squat offset + jump offset
        nextCenter.y = standingCenterY + currentSquatOffset + jumpOffset;

        capsule.center = nextCenter;
    }

    private float GetJumpOffset()
    {
        if (!isJumping)
        {
            return 0f;
        }

        jumpTimer += Time.deltaTime;

        float normalizedTime = jumpTimer / jumpDuration;

        if (normalizedTime >= 1f)
        {
            isJumping = false;
            jumpTimer = 0f;
            return 0f;
        }

        return Mathf.Sin(normalizedTime * Mathf.PI) * jumpColliderOffset;
    }
}