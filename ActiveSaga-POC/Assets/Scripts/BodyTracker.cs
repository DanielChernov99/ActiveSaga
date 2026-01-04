using UnityEngine;

public class BodyTracker : MonoBehaviour
{
    [Header("Tracked Transforms")]
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;


    //get the positions in real-time
    public Vector3 HeadPosition => head != null ? head.position : Vector3.zero;
    public Vector3 LeftHandPosition => leftHand != null ? leftHand.position : Vector3.zero;
    public Vector3 RightHandPosition => rightHand != null ? rightHand.position : Vector3.zero;

    void Awake()
    {
        // Check for null references to prevent errors
        if (head == null)
        {
            Debug.LogWarning("BodyTracker: Head transform not assigned!");
        }
    }
}
