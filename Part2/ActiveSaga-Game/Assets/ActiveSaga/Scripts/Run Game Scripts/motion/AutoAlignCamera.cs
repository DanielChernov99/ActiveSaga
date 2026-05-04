using UnityEngine;
using Unity.XR.CoreUtils;

public class AutoAlignCamera : MonoBehaviour
{
    [Header("Settings")]
    public Transform characterAnimator; // Drag your character here (KASA, GEKKOU, etc.)
    public float heightCorrection = 0.1f; // Small tweak if eyes feel too low/high

    void Start()
    {
        AlignCameraToCharacter();
    }

    void AlignCameraToCharacter()
    {
        if (characterAnimator == null)
        {
            // Try to find the player automatically if not assigned
            var animator = FindFirstObjectByType<Animator>();
            if (animator != null)
            {
                characterAnimator = animator.transform;
            }
            else
            {
                Debug.LogError("AutoAlignCamera: No Character found!");
                return;
            }
        }

        // 1. Get the Animator component to find the exact eye position
        Animator anim = characterAnimator.GetComponent<Animator>();
        if (anim == null) return;

        // 2. Find the head bone position
        Transform headBone = anim.GetBoneTransform(HumanBodyBones.Head);
        if (headBone == null) return;

        // 3. Find the XR Origin components
        XROrigin xrOrigin = FindFirstObjectByType<XROrigin>();
        if (xrOrigin == null) return;

        Transform cameraOffset = xrOrigin.CameraFloorOffsetObject.transform;
        
        // 4. Calculate the height difference
        // We want the Camera Y to match the Head Bone Y
        
        float characterHeadHeight = headBone.position.y;
        float currentCameraHeight = Camera.main.transform.position.y;
        
        // Calculate how much we need to move the "Camera Offset" container
        float heightDifference = characterHeadHeight - currentCameraHeight;

        // 5. Apply the movement to the Camera Offset
        Vector3 newPosition = cameraOffset.position;
        newPosition.y += heightDifference + heightCorrection;
        cameraOffset.position = newPosition;

        Debug.Log($"Calibrated! Moved camera by {heightDifference} to match character height: {characterHeadHeight}");
    }
}