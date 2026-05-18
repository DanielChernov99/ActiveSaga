using UnityEngine;
using System;

public class PlayerCollisionHandler : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How many seconds the player is immune after a hit")]
    [SerializeField] private float gracePeriod = 2.0f;

    public event Action OnObstacleCrash;

    private float lastCrashTime = -10f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle_Crash"))
        {
            HandleCrash();
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Obstacle_Crash"))
        {
            HandleCrash();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle_Crash"))
        {
            HandleCrash();
        }
    }

    private void HandleCrash()
    {
        if (Time.time - lastCrashTime < gracePeriod)
        {
            return;
        }

        lastCrashTime = Time.time;

        Debug.Log("CRASH! Hit the main obstacle.");
        OnObstacleCrash?.Invoke();
    }
}