using UnityEngine;
using ActiveSaga.BossFight.Core;
using ActiveSaga.BossFight.Data;

namespace ActiveSaga.BossFight.Entities
{
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyController : MonoBehaviour
    {
        public enum EnemyState { Idle, Spawning, Moving, Hit, Dead }

        [Header("State")]
        [SerializeField] private EnemyState currentState = EnemyState.Idle;

        private EnemyData data;
        private Rigidbody rb;
        private bool isInitialized;
        private float currentSpeed;
        private Vector3 targetOffset;

        private Transform targetCamera;
        private Transform targetRoot;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;

            // Must not be kinematic for physics movement
            rb.isKinematic = false;

            // Smooth movement
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public void Initialize(EnemyData enemyData, float speedMultiplier = 1f)
        {
            if (enemyData == null)
            {
                Debug.LogError("EnemyController: Initialized with null EnemyData!");
                return;
            }

            data = enemyData;
            currentSpeed = data.moveSpeed * speedMultiplier;

            // Add random horizontal offset (1.0 to 1.5m)
            float side = Random.value > 0.5f ? 1f : -1f;
            targetOffset = new Vector3(Random.Range(1.0f, 1.5f) * side, 0, 0);

            isInitialized = true;
            currentState = EnemyState.Moving;

            if (BossFightGameManager.Instance != null)
            {
                // We track both to follow the physical player position (camera XZ) but keep Y grounded
                targetCamera = BossFightGameManager.Instance.PlayerCamera != null ? BossFightGameManager.Instance.PlayerCamera.transform : null;
                targetRoot = BossFightGameManager.Instance.PlayerTransform;
            }

            EventManager.Trigger(new EnemySpawnedEvent { enemy = gameObject });
        }

        private void FixedUpdate()
        {
            if (!isInitialized || currentState != EnemyState.Moving) return;
            
            // Default movement direction if target is lost
            Vector3 direction = transform.forward;
            Vector3 targetPos = transform.position;

            bool hasTarget = false;

            if (targetCamera != null)
            {
                targetPos = targetCamera.position;
                hasTarget = true;
            }
            else if (targetRoot != null)
            {
                targetPos = targetRoot.position;
                hasTarget = true;
            }

            // If a player target exists, calculate precise direction towards it with offset
            if (hasTarget)
            {
                targetPos.y = transform.position.y; // Keep movement purely horizontal
                
                // Convert horizontal offset from player local space to world space if needed, 
                // or just apply it relative to the player's position.
                // Simple world-space horizontal offset based on current view direction:
                Vector3 playerRight = Vector3.Cross(Vector3.up, (targetPos - transform.position).normalized);
                Vector3 offsetPos = targetPos + (playerRight * targetOffset.x);

                direction = (offsetPos - rb.position).normalized;
            }

            // Move the Rigidbody forward regardless of target tracking status
            Vector3 newPos = rb.position + direction * currentSpeed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);

            // Rotate smoothly to face the movement direction
            if (direction != Vector3.zero)
            {
                rb.MoveRotation(Quaternion.LookRotation(direction));
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Sword") || other.CompareTag("Weapon"))
            {
                Despawn(true);
            }
        }

        public void Despawn(bool killedByPlayer)
        {
            if (currentState == EnemyState.Dead) return;

            currentState = EnemyState.Dead;

            EventManager.Trigger(new EnemyDespawnedEvent
            {
                enemy = gameObject,
                wasKilledByPlayer = killedByPlayer
            });

            if (PoolManager.Instance != null)
            {
                PoolManager.Instance.ReturnToPool(gameObject, data.enemyName);
            }
        }
    }
}