using UnityEngine;
using ActiveSaga.BossFight.Core;
using ActiveSaga.BossFight.Data;

namespace ActiveSaga.BossFight.Entities
{
    [RequireComponent(typeof(Rigidbody))]
    public class ProjectileController : MonoBehaviour
    {
        private ProjectileData data;
        private Rigidbody rb;

        private bool isInitialized;

        private Vector3 startPosition;
        private Vector3 forward;
        private Vector3 right;

        private float spawnTime;

        private Vector3 linearVelocity;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            ConfigureRigidbody();
        }

        private void ConfigureRigidbody()
        {
            rb.isKinematic = false;
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public string GetPoolName() =>
            data != null ? data.projectileName : string.Empty;

        public void Initialize(ProjectileData projectileData, float speedMultiplier = 1f)
        {
            if (projectileData == null) return;

            data = projectileData;
            isInitialized = true;

            spawnTime = Time.time;

            startPosition = transform.position;

            // Ensure we have a valid forward direction
            forward = transform.forward.normalized;
            if (forward.sqrMagnitude < 0.001f)
            {
                forward = Vector3.forward;
            }
            
            right = Vector3.Cross(Vector3.up, forward).normalized;
            if (right.sqrMagnitude < 0.001f)
            {
                right = Vector3.right;
            }

            // --- CRITICAL RESET SEQUENCE ---
            // Force wake up and clear all physics states for pooling consistency
            rb.isKinematic = false;
            rb.WakeUp();
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            // --------------------------------

            if (data.pattern == ProjectilePattern.Linear)
            {
                float targetSpeed = data.speed * speedMultiplier;
                if (targetSpeed <= 0) targetSpeed = 10f; // Safety fallback
                
                linearVelocity = forward * targetSpeed;
                rb.linearVelocity = linearVelocity;
            }

            EventManager.Trigger(new ProjectileSpawnedEvent
            {
                projectile = gameObject
            });

            CancelInvoke(nameof(DespawnDueToLifetime));
            Invoke(nameof(DespawnDueToLifetime), data.lifetime);
        }

        private void FixedUpdate()
        {
            if (!isInitialized || data == null) return;

            float t = Time.time - spawnTime;

            switch (data.pattern)
            {
                case ProjectilePattern.Linear:
                    // Keep constant velocity
                    rb.linearVelocity = linearVelocity;
                    break;

                case ProjectilePattern.Sine:
                    float sineOffset =
                        Mathf.Sin(t * data.frequency) * data.amplitude;

                    Vector3 targetPos =
                        startPosition +
                        forward * (data.speed * t) +
                        right * sineOffset;

                    rb.MovePosition(targetPos);
                    break;
            }
        }

        private void DespawnDueToLifetime()
        {
            DespawnInternal(true, false);
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(DespawnDueToLifetime));
            isInitialized = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isInitialized) return;

            if (other.CompareTag("PlayerHitbox") ||
                other.CompareTag("MainCamera") ||
                other.CompareTag("Player"))
            {
                if (BossFightGameManager.Instance != null)
                    BossFightGameManager.Instance.TakeDamage(data.damage);

                DespawnInternal(false, true);
                return;
            }

            // Sword does not destroy dodge objects anymore
            if (other.CompareTag("Sword"))
            {
                return;
            }

            if (other.CompareTag("DodgeShield"))
            {
                DespawnInternal(true, false);
            }
        }

        public void Despawn()
        {
            DespawnInternal(true, false);
        }

        private void DespawnInternal(bool wasDodged, bool hitPlayer)
        {
            if (!isInitialized)
                return;

            isInitialized = false;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            EventManager.Trigger(new ProjectileDespawnedEvent
            {
                projectile = gameObject,
                wasDodged = wasDodged && !hitPlayer,
                wasHitPlayer = hitPlayer
            });

            if (PoolManager.Instance != null)
                PoolManager.Instance.ReturnToPool(gameObject, data.projectileName);
            else
                Destroy(gameObject);
        }
    }
}