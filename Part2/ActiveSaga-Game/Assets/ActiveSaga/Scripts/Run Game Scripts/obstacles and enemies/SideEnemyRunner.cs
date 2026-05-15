using UnityEngine;
using ActiveSaga.RunGame;

public class SideEnemyRunner : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float destroyAfterSeconds = 8f;

    [Header("Kill Settings")]
    [SerializeField] private string weaponTag = "Sword";

    private Transform target;
    private RunGameStatsTracker statsTracker;

    public void Initialize(Transform playerTarget, RunGameStatsTracker tracker)
    {
        target = playerTarget;
        statsTracker = tracker;

        Destroy(gameObject, destroyAfterSeconds);
    }

    private void Update()
    {
        if (target == null)
        {
            return;
        }

        Vector3 targetPosition = target.position;
        targetPosition.y = transform.position.y;

        Vector3 direction = (targetPosition - transform.position).normalized;

        transform.position += direction * moveSpeed * Time.deltaTime;

        if (direction != Vector3.zero)
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