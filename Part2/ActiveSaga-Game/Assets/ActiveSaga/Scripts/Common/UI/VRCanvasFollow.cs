using UnityEngine;

public class VRCanvasFollow : MonoBehaviour
{
    public enum CanvasPositionPreset
    {
        FrontCenter,
        FrontRight,
        FrontLeft,
        Custom
    }

    [Header("Target")]
    [SerializeField] private Transform headTransform;
    [SerializeField] private bool findMainCameraAutomatically = true;

    [Header("Preset")]
    [SerializeField] private CanvasPositionPreset positionPreset = CanvasPositionPreset.Custom;

    [Header("Custom Offset Relative To Head")]
    [SerializeField] private Vector3 customLocalOffset = new Vector3(0f, -0.1f, 1.4f);

    [Header("Preset Offsets")]
    [SerializeField] private Vector3 frontCenterOffset = new Vector3(0f, -0.05f, 1.45f);
    [SerializeField] private Vector3 frontRightOffset = new Vector3(0.45f, -0.12f, 1.25f);
    [SerializeField] private Vector3 frontLeftOffset = new Vector3(-0.45f, -0.12f, 1.25f);

    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 8f;
    [SerializeField] private bool followPosition = true;
    [SerializeField] private bool followRotation = true;
    [SerializeField] private bool onlyUseHeadYaw = true;

    [Header("Rotation Settings")]
    [SerializeField] private bool faceSameDirectionAsHead = true;
    [SerializeField] private Vector3 rotationOffsetEuler = Vector3.zero;

    [Header("Snap")]
    [SerializeField] private bool snapOnEnable = true;

    private void Awake()
    {
        TryFindHeadTransform();
    }

    private void OnEnable()
    {
        TryFindHeadTransform();

        if (snapOnEnable)
        {
            SnapToTarget();
        }
    }

    private void LateUpdate()
    {
        if (headTransform == null)
        {
            TryFindHeadTransform();

            if (headTransform == null)
                return;
        }

        Vector3 targetPosition = GetTargetPosition();
        Quaternion targetRotation = GetTargetRotation();

        if (followPosition)
        {
            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                Time.deltaTime * followSpeed
            );
        }

        if (followRotation)
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * followSpeed
            );
        }
    }

    private void TryFindHeadTransform()
    {
        if (headTransform != null)
            return;

        if (!findMainCameraAutomatically)
            return;

        Camera mainCamera = Camera.main;

        if (mainCamera != null)
        {
            headTransform = mainCamera.transform;
        }
    }

    private Vector3 GetTargetPosition()
    {
        Quaternion yawRotation = GetYawRotation();
        Vector3 localOffset = GetCurrentOffset();

        return headTransform.position + yawRotation * localOffset;
    }

    private Quaternion GetTargetRotation()
    {
        Quaternion baseRotation;

        if (faceSameDirectionAsHead)
        {
            baseRotation = GetYawRotation();
        }
        else
        {
            Vector3 directionToHead = headTransform.position - transform.position;
            directionToHead.y = 0f;

            if (directionToHead.sqrMagnitude < 0.001f)
            {
                baseRotation = GetYawRotation();
            }
            else
            {
                baseRotation = Quaternion.LookRotation(-directionToHead.normalized, Vector3.up);
            }
        }

        return baseRotation * Quaternion.Euler(rotationOffsetEuler);
    }

    private Quaternion GetYawRotation()
    {
        if (!onlyUseHeadYaw)
        {
            return headTransform.rotation;
        }

        Vector3 flatForward = Vector3.ProjectOnPlane(headTransform.forward, Vector3.up).normalized;

        if (flatForward.sqrMagnitude < 0.001f)
        {
            flatForward = Vector3.forward;
        }

        return Quaternion.LookRotation(flatForward, Vector3.up);
    }

    private Vector3 GetCurrentOffset()
    {
        switch (positionPreset)
        {
            case CanvasPositionPreset.FrontCenter:
                return frontCenterOffset;

            case CanvasPositionPreset.FrontRight:
                return frontRightOffset;

            case CanvasPositionPreset.FrontLeft:
                return frontLeftOffset;

            case CanvasPositionPreset.Custom:
            default:
                return customLocalOffset;
        }
    }

    private void SnapToTarget()
    {
        if (headTransform == null)
            return;

        transform.position = GetTargetPosition();
        transform.rotation = GetTargetRotation();
    }
}