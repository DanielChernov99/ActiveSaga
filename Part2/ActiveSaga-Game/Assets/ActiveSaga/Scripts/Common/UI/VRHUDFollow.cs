using UnityEngine;

public class VRHUDFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform headTransform;

    [Header("Offset Relative To Head")]
    [SerializeField] private Vector3 localOffset = new Vector3(-0.35f, -0.08f, 0.85f);

    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 8f;
    [SerializeField] private bool followRotation = true;

    private void LateUpdate()
    {
        if (headTransform == null)
        {
            return;
        }

        Vector3 flatForward = Vector3.ProjectOnPlane(headTransform.forward, Vector3.up).normalized;

        if (flatForward.sqrMagnitude < 0.001f)
        {
            flatForward = Vector3.forward;
        }

        Quaternion yawRotation = Quaternion.LookRotation(flatForward, Vector3.up);

        Vector3 targetPosition = headTransform.position + yawRotation * localOffset;

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