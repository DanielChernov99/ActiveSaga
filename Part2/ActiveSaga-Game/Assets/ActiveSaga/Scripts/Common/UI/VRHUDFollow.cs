using UnityEngine;

public class VRHUDFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform headTransform;

    [Tooltip("Use this for forward direction. In Run Game, use XR Origin / Player Root / Run Forward Transform.")]
    [SerializeField] private Transform forwardReference;

    [Header("Offset Relative To Forward Direction")]
    [SerializeField] private Vector3 localOffset = new Vector3(0f, -0.1f, 1.1f);

    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 8f;
    [SerializeField] private bool followRotation = true;

    [Header("Pause Behavior")]
    [SerializeField] private bool snapOnEnable = true;
    [SerializeField] private bool followWhilePaused = false;

    private void OnEnable()
    {
        if (snapOnEnable)
        {
            SnapToTarget();
        }
    }

    private void LateUpdate()
    {
        if (headTransform == null)
        {
            return;
        }

        if (Time.timeScale == 0f && !followWhilePaused)
        {
            return;
        }

        GetTargetPose(out Vector3 targetPosition, out Quaternion targetRotation);

        float deltaTime = Time.timeScale == 0f
            ? Time.unscaledDeltaTime
            : Time.deltaTime;

        float t = Mathf.Clamp01(deltaTime * followSpeed);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            t
        );

        if (followRotation)
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                t
            );
        }
    }

    public void SnapToTarget()
    {
        if (headTransform == null)
        {
            return;
        }

        GetTargetPose(out Vector3 targetPosition, out Quaternion targetRotation);

        transform.position = targetPosition;

        if (followRotation)
        {
            transform.rotation = targetRotation;
        }
    }

    private void GetTargetPose(out Vector3 targetPosition, out Quaternion yawRotation)
    {
        Transform directionSource = forwardReference != null ? forwardReference : headTransform;

        Vector3 flatForward = Vector3.ProjectOnPlane(directionSource.forward, Vector3.up).normalized;

        if (flatForward.sqrMagnitude < 0.001f)
        {
            flatForward = Vector3.forward;
        }

        yawRotation = Quaternion.LookRotation(flatForward, Vector3.up);

        targetPosition = headTransform.position + yawRotation * localOffset;
    }
}