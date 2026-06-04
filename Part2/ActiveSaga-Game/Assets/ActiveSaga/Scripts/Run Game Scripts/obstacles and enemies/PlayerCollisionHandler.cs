using UnityEngine;
using System;

public class PlayerCollisionHandler : MonoBehaviour
{
    private const string ObstacleCrashTag = "Obstacle_Crash";

    [Header("Settings")]
    [Tooltip("How many seconds the player is immune after a hit")]
    [SerializeField] private float gracePeriod = 2.0f;

    public event Action OnObstacleCrash;

    private float lastCrashTime = -10f;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        TryHandleCrash(hit.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHandleCrash(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryHandleCrash(collision.gameObject);
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
}