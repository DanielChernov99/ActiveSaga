using UnityEngine;
using ActiveSaga.BossFight.Core;
using ActiveSaga.BossFight.Data;

namespace ActiveSaga.BossFight.Entities
{
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyController : MonoBehaviour
    {
        public enum EnemyState { Idle, Moving, Dead }

        [Header("State")]
        [SerializeField] private EnemyState currentState = EnemyState.Idle;

        private EnemyData data;
        private Rigidbody rb;

        private bool isInitialized;
        private bool wasKilledByPlayer;

        private float currentSpeed;
        private Vector3 moveDirection;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        public void Initialize(EnemyData enemyData, float speedMultiplier = 1f)
        {
            if (enemyData == null)
                return;

            data = enemyData;
            isInitialized = true;
            wasKilledByPlayer = false;

            currentState = EnemyState.Moving;

            float finalSpeed = data.moveSpeed * speedMultiplier;
            if (finalSpeed <= 0f)
                finalSpeed = 5f;

            currentSpeed = finalSpeed;

            // Calculate direction ONCE only
            Vector3 targetPosition = transform.position + transform.forward * 20f;

            if (BossFightGameManager.Instance != null &&
                BossFightGameManager.Instance.PlayerTransform != null)
            {
                targetPosition =
                    BossFightGameManager.Instance.PlayerTransform.position;
                targetPosition.y = transform.position.y;
            }

            moveDirection =
                (targetPosition - transform.position).normalized;

            if (moveDirection.sqrMagnitude < 0.001f)
                moveDirection = transform.forward;

            transform.rotation = Quaternion.LookRotation(moveDirection);

            // Reset physics
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.WakeUp();

            EventManager.Trigger(new EnemySpawnedEvent
            {
                enemy = gameObject
            });

            CancelInvoke(nameof(DespawnDueToLifetime));
           
        }

        private void FixedUpdate()
        {
            if (!isInitialized)
                return;

            if (currentState != EnemyState.Moving)
                return;

            // Move ONLY in the original direction
            Vector3 newPosition =
                rb.position +
                moveDirection * currentSpeed * Time.fixedDeltaTime;

            rb.MovePosition(newPosition);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isInitialized)
                return;

            // Sword kills enemy
            if (other.CompareTag("Sword") ||
                other.CompareTag("Weapon"))
            {
                Despawn(true);
                return;
            }
        }

        private void DespawnDueToLifetime()
        {
            Despawn(false);
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(DespawnDueToLifetime));
            isInitialized = false;
            currentState = EnemyState.Idle;
        }

        public void Despawn(bool killedByPlayer)
        {
            if (!isInitialized)
                return;

            if (currentState == EnemyState.Dead)
                return;

            isInitialized = false;
            currentState = EnemyState.Dead;
            wasKilledByPlayer = killedByPlayer;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            EventManager.Trigger(new EnemyDespawnedEvent
            {
                enemy = gameObject,
                wasKilledByPlayer = killedByPlayer
            });

            if (PoolManager.Instance != null)
            {
                PoolManager.Instance.ReturnToPool(
                    gameObject,
                    data.enemyName
                );
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}