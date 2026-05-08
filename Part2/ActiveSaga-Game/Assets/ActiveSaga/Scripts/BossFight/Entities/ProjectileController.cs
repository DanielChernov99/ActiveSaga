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
        private bool isInitialized = false;

        private bool wasHitPlayer = false;
        private Vector3 startPosition;
        private Vector3 initialForward;
        private float spawnTime;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = false; 
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        public string GetPoolName() => data != null ? data.projectileName : string.Empty;

        private Vector3 targetVelocity;

        public void Initialize(ProjectileData projectileData, float speedMultiplier = 1f)
        {
            if (projectileData == null) return;

            this.data = projectileData;
            isInitialized = true;
            wasHitPlayer = false;
            startPosition = transform.position;
            initialForward = transform.forward;
            spawnTime = Time.time;

            // Ensure Rigidbody is ready and active
            if (rb == null) rb = GetComponent<Rigidbody>();
            
            // Force physics to be active
            rb.isKinematic = false; 
            rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.WakeUp(); // Ensure the Rigidbody is awake
            
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Apply velocity for linear movement
            if (data.pattern == ProjectilePattern.Linear)
            {
                targetVelocity = initialForward * data.speed * speedMultiplier;
                
                // If speed is 0 or forward is zero, provide a default
                if (targetVelocity.sqrMagnitude < 0.01f)
                {
                    targetVelocity = transform.forward * 10f;
                }
                
                rb.linearVelocity = targetVelocity;
            }

            EventManager.Trigger(new ProjectileSpawnedEvent { projectile = gameObject });
            
            // Return to pool after lifetime
            CancelInvoke(nameof(DespawnDueToLifetime));
            Invoke(nameof(DespawnDueToLifetime), data.lifetime);
        }

        private void FixedUpdate()
        {
            if (!isInitialized || data == null) return;

            if (data.pattern == ProjectilePattern.Sine)
            {
                // Add Sine movement offset
                float timeActive = Time.time - spawnTime;
                float sineOffset = Mathf.Sin(timeActive * data.frequency) * data.amplitude;
                
                // Calculate side vector (perpendicular to forward and up)
                Vector3 side = Vector3.Cross(initialForward, Vector3.up).normalized;
                
                // Target position based on linear progress + sine offset
                Vector3 targetPos = startPosition + (initialForward * (data.speed * (data.speed != 0 ? 1f : 0f)) * timeActive) + (side * sineOffset);
                rb.MovePosition(targetPos);
            }
            else if (data.pattern == ProjectilePattern.Linear)
            {
                // Safety check: ensure velocity is maintained
                // Using rb.velocity as fallback for linearVelocity if needed, but linearVelocity is correct for Unity 6
                if (rb.linearVelocity.sqrMagnitude < 0.1f && targetVelocity != Vector3.zero)
                {
                    rb.linearVelocity = targetVelocity;
                }
                
                // Manual backup movement if physics fails to update position
                // rb.MovePosition(rb.position + targetVelocity * Time.fixedDeltaTime);
            }
        }

        private void DespawnDueToLifetime()
        {
            // If it lived its full lifetime without hitting player, it's a successful dodge
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

            // Check if hit player (XR Headset or Body Collider)
            if (other.CompareTag("PlayerHitbox") || other.CompareTag("MainCamera") || other.CompareTag("Player"))
            {
                Debug.Log($"<color=orange>Projectile Hit Player: {data.projectileName}</color>");
                if (BossFightGameManager.Instance != null)
                {
                    BossFightGameManager.Instance.TakeDamage(data.damage);
                }
                wasHitPlayer = true;
                DespawnInternal(false, true);
            }
            // Check if deflected (Combat wave or specific mechanic)
            else if (other.CompareTag("Sword") || other.CompareTag("DodgeShield"))
            {
                Debug.Log($"Projectile {data.projectileName} deflected.");
                DespawnInternal(true, false);
            }
        }

        public void Despawn()
        {
            // Called by boundary or external force
            DespawnInternal(true, false);
        }

        private void DespawnInternal(bool wasDodged, bool wasHitPlayer)
        {
            if (!isInitialized) return;
            isInitialized = false;
            
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            EventManager.Trigger(new ProjectileDespawnedEvent { 
                projectile = gameObject, 
                wasDodged = wasDodged && !wasHitPlayer, 
                wasHitPlayer = wasHitPlayer 
            });
            
            if (PoolManager.Instance != null)
                PoolManager.Instance.ReturnToPool(gameObject, data.projectileName);
            else
                Destroy(gameObject);
        }
    }
}


