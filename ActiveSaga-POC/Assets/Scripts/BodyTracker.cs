using UnityEngine;

public class BodyTracker : MonoBehaviour
{
    [Header("Tracked Transforms")]
    // Using [SerializeField] private protects these variables from other scripts
    [SerializeField] private Transform head;
    [SerializeField] private Transform leftHand;
    [SerializeField] private Transform rightHand;

    // Public properties provide read-only access to positions
    public Vector3 HeadPosition => head ? head.position : Vector3.zero;
    public Vector3 LeftHandPosition => leftHand ? leftHand.position : Vector3.zero;
    public Vector3 RightHandPosition => rightHand ? rightHand.position : Vector3.zero;

    private void Awake()
    {
        // Your safety check is excellent, keep it!
        if (head == null)
        {
            Debug.LogWarning("BodyTracker: Head transform not assigned! Please assign it in the Inspector.");
        }
    }
}