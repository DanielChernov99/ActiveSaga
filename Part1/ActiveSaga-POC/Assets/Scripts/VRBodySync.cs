using UnityEngine;

public class VRBodySync : MonoBehaviour
{
    [Header("Status (Auto-Filled)")]
    public Transform headTarget;      
    public Transform leftHandTarget;  
    public Transform rightHandTarget; 

    [Header("Settings")]
    public Vector3 bodyOffset = new Vector3(0, 0, 0); 
    public float turnSmoothness = 5f;

    [Tooltip("Distance to push the body back, so you don't see your own insides.")]
    public float spineOffset = 0.2f; // <-- התיקון החדש: מרחיק את הגוף אחורה

    [Header("Hands Rotation Correction")]
    public Vector3 leftHandRotOffset;  
    public Vector3 rightHandRotOffset; 

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        
        // --- Automation: Find parts automatically ---
        AutoFindTargets();
    }

    void AutoFindTargets()
    {
        // 1. Find Head (Main Camera)
        if (headTarget == null)
        {
            if (Camera.main != null) headTarget = Camera.main.transform;
            else Debug.LogError("VRBodySync: Main Camera not found in scene!");
        }

        // 2. Find Controllers
        var xrOrigin = FindFirstObjectByType<Unity.XR.CoreUtils.XROrigin>();
        if (xrOrigin != null)
        {
            if (leftHandTarget == null) leftHandTarget = FindChildRecursive(xrOrigin.transform, "Left Controller");
            if (rightHandTarget == null) rightHandTarget = FindChildRecursive(xrOrigin.transform, "Right Controller");
        }
        else
        {
            if (leftHandTarget == null) leftHandTarget = GameObject.Find("Left Controller")?.transform;
            if (rightHandTarget == null) rightHandTarget = GameObject.Find("Right Controller")?.transform;
        }
    }

    Transform FindChildRecursive(Transform parent, string nameToFind)
    {
        foreach (Transform child in parent)
        {
            if (child.name == nameToFind) return child;
            Transform result = FindChildRecursive(child, nameToFind);
            if (result != null) return result;
        }
        return null;
    }

    void LateUpdate()
    {
        if (headTarget == null) return;

        // --- 1. Calculate Rotation & Direction ---
        Vector3 lookDir = headTarget.forward;
        lookDir.y = 0; // Flatten the direction (ignore height)

        // --- 2. Calculate Position with Spine Offset ---
        // Take camera position, and move BACKWARDS based on spineOffset
        Vector3 targetPosition = headTarget.position - (lookDir.normalized * spineOffset);
        
        // Keep the feet on the ground (maintain original Y)
        targetPosition.y = transform.position.y; 

        // Apply Position
        transform.position = targetPosition + bodyOffset;

        // --- 3. Apply Rotation ---
        if (lookDir.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * turnSmoothness);
        }
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;

        if (headTarget != null)
        {
            animator.SetLookAtWeight(1);
            animator.SetLookAtPosition(headTarget.position + headTarget.forward * 2f);
        }

        if (leftHandTarget != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation * Quaternion.Euler(leftHandRotOffset));
        }

        if (rightHandTarget != null)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTarget.rotation * Quaternion.Euler(rightHandRotOffset));
        }
    }
}