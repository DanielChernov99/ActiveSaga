using UnityEngine;
using System;

public class PlayerCollisionHandler : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How many seconds the player is immune after a hit")]
    public float gracePeriod = 2.0f;

    public event Action OnObstacleCrash;
    public event Action OnObstacleGraze;

    private float lastCrashTime = -10f;
    private float lastGrazeTime = -10f;

    // רץ אם השחקן משתמש ב-Rigidbody ו-Collider רגילים
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle_Crash"))
        {
            HandleCrash();
        }
    }

    // רץ אם השחקן משתמש ב-Character Controller (נפוץ מאוד ב-XR)
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Obstacle_Crash"))
        {
            HandleCrash();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle_Graze"))
        {
            HandleGraze();
        }
    }

    private void HandleCrash()
    {
        // בדיקת חסינות של התרסקות
        if (Time.time - lastCrashTime < gracePeriod) return;

        lastCrashTime = Time.time;
        Debug.Log("CRASH! Hit the main obstacle.");
        OnObstacleCrash?.Invoke();
    }

    private void HandleGraze()
    {
        // אם התרסקנו ממש בשבריר השנייה האחרון, זה לא שפשוף אלא התרסקות! תתעלם מהשפשוף.
        if (Time.time - lastCrashTime < 0.1f) return;

        // בדיקת חסינות של שפשוף
        if (Time.time - lastGrazeTime < gracePeriod) return;

        lastGrazeTime = Time.time;
        Debug.Log("GRAZE! Clipped the top of the obstacle.");
        OnObstacleGraze?.Invoke();
    }
}