using UnityEngine;

namespace ActiveSaga.BossFight
{
    public class ObstacleProjectile : MonoBehaviour
    {
        [Header("Settings")]
        public float speed = 5f;
        
        private Vector3 direction;
        private BossFightManager manager;
        private bool isResolved = false;

        public void Initialize(Vector3 targetPosition, float speed, BossFightManager bossManager)
        {
            this.speed = speed;
            this.manager = bossManager;
            
            // Calculate direction toward target
            direction = (targetPosition - transform.position).normalized;
            
            // Ensure it doesn't just stop at the target but continues through
            // However, the logic for "Dodge" is hitting the SafeZone wall behind.
        }

        private void Update()
        {
            if (isResolved) return;
            transform.position += direction * speed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isResolved) return;

            // 1. Check if hit the player
            if (other.CompareTag("PlayerHitbox") || other.CompareTag("MainCamera"))
            {
                Resolve(false);
            }
            // 2. Check if reached the SafeZone (dodged)
            else if (other.CompareTag("SafeZone"))
            {
                Resolve(true);
            }
        }

        private void Resolve(bool success)
        {
            isResolved = true;
            if (success)
            {
                manager.ReportSuccess();
            }
            else
            {
                manager.ReportFailure();
            }
            Destroy(gameObject);
        }
    }
}
