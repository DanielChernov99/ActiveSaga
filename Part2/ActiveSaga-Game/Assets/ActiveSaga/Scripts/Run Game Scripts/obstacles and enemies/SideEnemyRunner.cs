using UnityEngine;
using ActiveSaga.RunGame;

public class SideEnemyRunner : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Activation")]
    [SerializeField] private float activationDistance = 35f;
    [SerializeField] private float destroyBehindPlayerDistance = 10f;

    [Header("Kill Settings")]
    [SerializeField] private string weaponTag = "Sword";

    private Transform target;
    private RunGameStatsTracker statsTracker;
    private bool isActivated = false;

    public void Initialize(Transform playerTarget, RunGameStatsTracker tracker)
    {
        target = playerTarget;
        statsTracker = tracker;

        if (target == null)
        {
            Debug.LogError("SideEnemyRunner: Missing player target.");
        }
    }

    private void Update()
    {
        if (target == null)
        {
            return;
        }

        float zDistanceFromPlayer = transform.position.z - target.position.z;

        if (!isActivated)
        {
            if (zDistanceFromPlayer > activationDistance)
            {
                return;
            }

            isActivated = true;
            Debug.Log("SideEnemy activated: " + gameObject.name);
        }

        if (zDistanceFromPlayer < -destroyBehindPlayerDistance)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPosition = target.position;
        targetPosition.y = transform.position.y;

        Vector3 direction = (targetPosition - transform.position).normalized;

        transform.position += direction * moveSpeed * Time.deltaTime;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(weaponTag))
        {
            return;
        }

        if (statsTracker != null)
        {
            statsTracker.AddEnemyKill();
        }

        Destroy(gameObject);
    }
}