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

        [Header("Animation")]
        [SerializeField] private string walkStateName = "Walk";

        private EnemyData data;
        private Rigidbody rb;
        private Animator animator;

        private bool isInitialized;
        private bool wasKilledByPlayer;

        private float currentSpeed;
        private Vector3 moveDirection;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            animator = GetComponentInChildren<Animator>();

            ConfigureRigidbody();
        }

        private void ConfigureRigidbody()
        {
            if (rb == null)
                return;

            rb.useGravity = false;
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

            rb.constraints =
                RigidbodyConstraints.FreezePositionY |
                RigidbodyConstraints.FreezeRotationX |
                RigidbodyConstraints.FreezeRotationY |
                RigidbodyConstraints.FreezeRotationZ;
        }

        public void Initialize(EnemyData enemyData, float speedMultiplier = 1f)
        {
            if (enemyData == null)
            {
                Debug.LogError($"EnemyController on {gameObject.name}: EnemyData is null.");
                return;
            }

            data = enemyData;
            isInitialized = true;
            wasKilledByPlayer = false;
            currentState = EnemyState.Moving;

            float finalSpeed = data.moveSpeed * speedMultiplier;

            if (finalSpeed <= 0f)
                finalSpeed = 5f;

            currentSpeed = finalSpeed;

            CalculateMoveDirection();
            ResetPhysics();
            PlayWalkAnimation();

            EventManager.Trigger(new EnemySpawnedEvent
            {
                enemy = gameObject
            });

            CancelInvoke(nameof(DespawnDueToLifetime));
        }

        private void CalculateMoveDirection()
        {
            Vector3 targetPosition = transform.position + transform.forward * 20f;

            if (BossFightGameManager.Instance != null &&
                BossFightGameManager.Instance.PlayerTransform != null)
            {
                targetPosition = BossFightGameManager.Instance.PlayerTransform.position;
                targetPosition.y = transform.position.y;
            }

            moveDirection = targetPosition - transform.position;
            moveDirection.y = 0f;

            if (moveDirection.sqrMagnitude < 0.001f)
            {
                moveDirection = transform.forward;
                moveDirection.y = 0f;
            }

            moveDirection.Normalize();

            if (moveDirection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection);
            }
        }

        private void ResetPhysics()
        {
            if (rb == null)
                return;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            ConfigureRigidbody();

            rb.WakeUp();
        }

        private void PlayWalkAnimation()
        {
            if (animator == null)
            {
                Debug.LogWarning($"EnemyController on {gameObject.name}: No Animator found in children.");
                return;
            }

            animator.enabled = true;
            animator.Rebind();
            animator.Update(0f);

            if (!string.IsNullOrEmpty(walkStateName))
            {
                animator.Play(walkStateName, 0, 0f);
            }
        }

        private void FixedUpdate()
        {
            if (!isInitialized)
                return;

            if (currentState != EnemyState.Moving)
                return;

            Vector3 newPosition =
                rb.position +
                moveDirection * currentSpeed * Time.fixedDeltaTime;

            newPosition.y = rb.position.y;

            rb.MovePosition(newPosition);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isInitialized)
                return;

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

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
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

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            EventManager.Trigger(new EnemyDespawnedEvent
            {
                enemy = gameObject,
                wasKilledByPlayer = killedByPlayer
            });

            if (PoolManager.Instance != null && data != null)
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