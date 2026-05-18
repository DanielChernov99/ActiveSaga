using UnityEngine;

public class VRForwardOnlyFollow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform headTransform;
    [SerializeField] private Transform positionCenter;

    [Header("Offset")]
    [SerializeField] private float forwardDistance = 0.85f;
    [SerializeField] private float heightOffset = -0.08f;

    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 8f;
    [SerializeField] private bool followRotation = true;

    private void LateUpdate()
    {
        if (headTransform == null)
        {
            return;
        }

        Transform center = positionCenter != null ? positionCenter : headTransform;

        Vector3 flatForward = Vector3.ProjectOnPlane(headTransform.forward, Vector3.up).normalized;

        if (flatForward.sqrMagnitude < 0.001f)
        {
            flatForward = Vector3.forward;
        }

        Quaternion yawRotation = Quaternion.LookRotation(flatForward, Vector3.up);

        Vector3 targetPosition =
            center.position +
            flatForward * forwardDistance +
            Vector3.up * heightOffset;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            Time.deltaTime * followSpeed
        );

        if (followRotation)
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                yawRotation,
                Time.deltaTime * followSpeed
            );
        }
    }
}