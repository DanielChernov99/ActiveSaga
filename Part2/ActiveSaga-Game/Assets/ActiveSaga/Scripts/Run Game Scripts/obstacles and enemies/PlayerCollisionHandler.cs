using UnityEngine;
using System;

public class PlayerCollisionHandler : MonoBehaviour
{
    private const string ObstacleCrashTag = "Obstacle_Crash";

    [Header("Settings")]
    [Tooltip("How many seconds the player is immune after a hit")]
    [SerializeField] private float gracePeriod = 2.0f;

    [Header("Jump Zone Settings")]
    [SerializeField] private JumpAnalyzer jumpAnalyzer;

    [Tooltip("Name of the trigger object used to check if the player jumped near an obstacle.")]
    [SerializeField] private string jumpCheckZoneName = "Jump_Check_Zone";

    [Tooltip("Allows a jump that happened slightly before entering the jump zone.")]
    [SerializeField] private float earlyJumpTolerance = 0.35f;

    [Tooltip("Destroy the obstacle when the player successfully jumps in the jump zone.")]
    [SerializeField] private bool destroyObstacleOnJumpSuccess = true;

    public event Action OnObstacleCrash;

    private float lastCrashTime = -10f;

    private GameObject currentJumpZone;
    private GameObject currentJumpZoneObstacle;

    private int jumpCountOnZoneEnter;
    private int currentJumpZoneEnterCount;

    private bool jumpAcceptedInZone;

    private void Awake()
    {

        if (jumpAnalyzer == null)
        {
            Debug.LogWarning(
                "[PlayerCollisionHandler] JumpAnalyzer is not assigned on: " +
                GetHierarchyPath(transform) +
                ". Jump zones will be ignored by this handler."
            );
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (BelongsToJumpZoneObstacle(hit.gameObject))
        {
            return;
        }

        TryHandleCrash(hit.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsJumpCheckZone(other.gameObject))
        {
            if (jumpAnalyzer == null)
            {
                Debug.LogWarning(
                    "[JumpZone] ENTER ignored because JumpAnalyzer is missing on handler=" +
                    GetHierarchyPath(transform)
                );

                return;
            }

            HandleJumpZoneEnter(other.gameObject);
            return;
        }

        if (BelongsToJumpZoneObstacle(other.gameObject))
        {
            return;
        }

        TryHandleCrash(other.gameObject);
    }

    private void OnTriggerStay(Collider other)
    {
        if (jumpAnalyzer == null)
        {
            return;
        }

        if (!IsCurrentJumpZone(other.gameObject))
        {
            return;
        }

        CheckJumpAccepted();
    }

    private void OnTriggerExit(Collider other)
    {
        if (jumpAnalyzer == null)
        {
            return;
        }

        if (!IsCurrentJumpZone(other.gameObject))
        {
            return;
        }

        currentJumpZoneEnterCount--;

        if (currentJumpZoneEnterCount > 0)
        {
            return;
        }

        CheckJumpAccepted();

        if (jumpAcceptedInZone)
        {
            Debug.Log(
                "[JumpZone] SUCCESS - player jumped in time. | handler=" +
                GetHierarchyPath(transform)
            );

            if (destroyObstacleOnJumpSuccess && currentJumpZoneObstacle != null)
            {
                GameObject objectToDestroy = GetSafeObjectToDestroy(currentJumpZoneObstacle);

                if (objectToDestroy != null)
                {
                    Destroy(objectToDestroy);
                }
            }
        }
        else
        {
            Debug.Log(
                "[JumpZone] FAIL - player did not jump. Stun triggered. | handler=" +
                GetHierarchyPath(transform)
            );

            if (currentJumpZoneObstacle != null)
            {
                HandleCrash(currentJumpZoneObstacle);
            }
            else
            {
                Debug.LogWarning("[JumpZone] No Obstacle_Crash object found near this Jump_Check_Zone.");
            }
        }

        ResetJumpZoneState();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (BelongsToJumpZoneObstacle(collision.gameObject))
        {
            return;
        }

        TryHandleCrash(collision.gameObject);
    }

    private void HandleJumpZoneEnter(GameObject zoneObject)
    {
        if (currentJumpZone != null)
        {
            if (currentJumpZone == zoneObject)
            {
                currentJumpZoneEnterCount++;

                CheckJumpAccepted();
                return;
            }

            Debug.LogWarning(
                "[JumpZone] Entered a new jump zone while another one is active. " +
                "OldZone=" + currentJumpZone.name +
                " | NewZone=" + zoneObject.name +
                " | handler=" + GetHierarchyPath(transform)
            );

            ResetJumpZoneState();
        }

        currentJumpZone = zoneObject;
        currentJumpZoneObstacle = FindObstacleCrashObject(zoneObject);

        jumpCountOnZoneEnter = jumpAnalyzer != null ? jumpAnalyzer.JumpCounter : 0;
        currentJumpZoneEnterCount = 1;
        jumpAcceptedInZone = false;

        CheckJumpAccepted();

    }

    private void CheckJumpAccepted()
    {
        if (jumpAnalyzer == null)
        {
            return;
        }

        bool jumpedAfterEnteringZone = jumpAnalyzer.JumpCounter > jumpCountOnZoneEnter;
        bool jumpedSlightlyBeforeZone = Time.time - jumpAnalyzer.LastJumpTime <= earlyJumpTolerance;

        if (jumpedAfterEnteringZone || jumpedSlightlyBeforeZone)
        {
            jumpAcceptedInZone = true;
        }
    }

    private void ResetJumpZoneState()
    {
        currentJumpZone = null;
        currentJumpZoneObstacle = null;

        jumpCountOnZoneEnter = 0;
        currentJumpZoneEnterCount = 0;

        jumpAcceptedInZone = false;
    }

    private bool IsCurrentJumpZone(GameObject obj)
    {
        return currentJumpZone != null && obj == currentJumpZone;
    }

    private bool IsJumpCheckZone(GameObject obj)
    {
        return obj != null && obj.name.Contains(jumpCheckZoneName);
    }

    private bool BelongsToJumpZoneObstacle(GameObject hitObject)
    {
        if (hitObject == null)
        {
            return false;
        }

        Transform current = hitObject.transform;

        while (current != null)
        {
            if (FindNamedChild(current, jumpCheckZoneName) != null)
            {
                return true;
            }

            if (current.name == "ContentRoot")
            {
                break;
            }

            if (current.GetComponent<TileInfo>() != null)
            {
                break;
            }

            current = current.parent;
        }

        return false;
    }

    private Transform FindNamedChild(Transform root, string childName)
    {
        Transform[] children = root.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].name.Contains(childName))
            {
                return children[i];
            }
        }

        return null;
    }

    private void TryHandleCrash(GameObject hitObject)
    {
        GameObject obstacleObject = FindObstacleCrashObject(hitObject);

        if (obstacleObject == null)
        {
            return;
        }

        HandleCrash(obstacleObject);
    }

    private GameObject FindObstacleCrashObject(GameObject hitObject)
    {
        if (hitObject == null)
        {
            return null;
        }

        // Check object itself and its parents
        Transform current = hitObject.transform;

        while (current != null)
        {
            if (current.CompareTag(ObstacleCrashTag))
            {
                return current.gameObject;
            }

            current = current.parent;
        }

        // Check siblings under parent/prefab root
        Transform parent = hitObject.transform.parent;

        while (parent != null)
        {
            if (parent.name == "ContentRoot")
            {
                break;
            }

            if (parent.GetComponent<TileInfo>() != null)
            {
                break;
            }

            GameObject foundObject = FindTaggedChild(parent, ObstacleCrashTag);

            if (foundObject != null)
            {
                return foundObject;
            }

            parent = parent.parent;
        }

        return null;
    }

    private GameObject FindTaggedChild(Transform root, string tagToFind)
    {
        Transform[] children = root.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].CompareTag(tagToFind))
            {
                return children[i].gameObject;
            }
        }

        return null;
    }

    private void HandleCrash(GameObject obstacleObject)
    {
        if (Time.time - lastCrashTime < gracePeriod)
        {
            return;
        }

        lastCrashTime = Time.time;

        OnObstacleCrash?.Invoke();

        GameObject objectToDestroy = GetSafeObjectToDestroy(obstacleObject);

        if (objectToDestroy != null)
        {
            Destroy(objectToDestroy);
        }
    }

    private GameObject GetSafeObjectToDestroy(GameObject hitObject)
    {
        if (hitObject == null)
        {
            return null;
        }

        Transform current = hitObject.transform;

        while (current.parent != null)
        {
            Transform parent = current.parent;

            if (parent.GetComponent<TileInfo>() != null)
            {
                return current.gameObject;
            }

            if (parent.name == "ContentRoot")
            {
                return current.gameObject;
            }

            current = parent;
        }

        return hitObject;
    }

    private string GetHierarchyPath(Transform target)
    {
        if (target == null)
        {
            return "NULL";
        }

        string path = target.name;

        while (target.parent != null)
        {
            target = target.parent;
            path = target.name + "/" + path;
        }

        return path;
    }
}