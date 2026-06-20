using System.Collections;
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
        [SerializeField] private string deathTrigger = "Death";

        [Header("Death Feedback")]
        [SerializeField] private AudioClip deathSound;
        [SerializeField] private float deathReturnDelay = 1.2f;

        [Header("Movement")]
        [SerializeField] private bool retargetPlayerWhileMoving = true;
        [SerializeField] private float retargetInterval = 0.1f;
        [SerializeField] private float rotationLerpSpeed = 10f;
        [SerializeField] private bool continueStraightAfterReachingPlayer = true;
        [SerializeField] private float stopRetargetingDistance = 1.2f;
        [SerializeField] private bool useRandomTargetLane = true;
        [SerializeField] private float targetLaneHalfWidth = 1.2f;
        [SerializeField] private float targetLaneMinAbs = 0.35f;

        [Header("Ground Follow")]
        [SerializeField] private LayerMask groundLayer = ~0;
        [SerializeField] private float groundRayHeight = 3f;
        [SerializeField] private float groundRayDistance = 10f;
        [SerializeField] private float groundOffset = 0f;

        private EnemyData data;
        private Rigidbody rb;
        private Animator animator;
        private Collider[] colliders;
        private Coroutine deathRoutine;

        private bool isInitialized;
        private bool wasKilledByPlayer;
        private bool hasReachedPlayer;

        private float currentSpeed;
        private float retargetTimer;
        private float targetSideOffset;
        private Vector3 moveDirection;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            animator = GetComponentInChildren<Animator>();
            colliders = GetComponentsInChildren<Collider>();

            ConfigureRigidbody();
        }

        private void ConfigureRigidbody()
        {
            if (rb == null)
            {
                return;
            }

            rb.useGravity = false;
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

            rb.constraints =
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
            hasReachedPlayer = false;
            currentState = EnemyState.Moving;
            retargetTimer = 0f;
            targetSideOffset = PickRandomTargetSideOffset();

            if (deathRoutine != null)
            {
                StopCoroutine(deathRoutine);
                deathRoutine = null;
            }

            SetCollidersEnabled(true);

            float finalSpeed = data.moveSpeed * speedMultiplier;

            if (finalSpeed <= 0f)
            {
                finalSpeed = 5f;
            }

            currentSpeed = finalSpeed;

            CalculateMoveDirection(true);
            ResetPhysics();
            SnapToGround();
            PlayWalkAnimation();

            EventManager.Trigger(new EnemySpawnedEvent
            {
                enemy = gameObject
            });

            CancelInvoke(nameof(DespawnDueToLifetime));
        }

        private float PickRandomTargetSideOffset()
        {
            if (!useRandomTargetLane || targetLaneHalfWidth <= 0f)
            {
                return 0f;
            }

            float offset = Random.Range(-targetLaneHalfWidth, targetLaneHalfWidth);

            if (Mathf.Abs(offset) < targetLaneMinAbs)
            {
                offset = targetLaneMinAbs * (Random.value < 0.5f ? -1f : 1f);
            }

            return offset;
        }

        private bool TryGetPlayerTargetPosition(out Vector3 targetPosition)
        {
            targetPosition = Vector3.zero;
            Transform targetTransform = null;

            if (BossFightGameManager.Instance != null &&
                BossFightGameManager.Instance.PlayerCamera != null)
            {
                targetTransform = BossFightGameManager.Instance.PlayerCamera.transform;
            }
            else if (Camera.main != null)
            {
                targetTransform = Camera.main.transform;
            }
            else if (BossFightGameManager.Instance != null &&
                     BossFightGameManager.Instance.PlayerTransform != null)
            {
                targetTransform = BossFightGameManager.Instance.PlayerTransform;
            }

            if (targetTransform == null)
            {
                return false;
            }

            targetPosition = targetTransform.position;

            Vector3 right = targetTransform.right;
            right.y = 0f;

            if (right.sqrMagnitude < 0.001f)
            {
                right = Vector3.right;
            }

            targetPosition += right.normalized * targetSideOffset;

            return true;
        }

        private void CalculateMoveDirection(bool snapRotation)
        {
            Vector3 currentPosition = rb != null ? rb.position : transform.position;
            Vector3 targetPosition = currentPosition + transform.forward * 20f;

            if (TryGetPlayerTargetPosition(out Vector3 playerTargetPosition))
            {
                targetPosition = playerTargetPosition;
                targetPosition.y = currentPosition.y;
            }

            Vector3 direction = targetPosition - currentPosition;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
            {
                direction = transform.forward;
                direction.y = 0f;
            }

            if (direction.sqrMagnitude < 0.001f)
            {
                direction = Vector3.forward;
            }

            moveDirection = direction.normalized;

            if (moveDirection.sqrMagnitude <= 0.001f)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);

            if (snapRotation)
            {
                transform.rotation = targetRotation;
                return;
            }

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationLerpSpeed * Time.fixedDeltaTime
            );
        }

        private void ResetPhysics()
        {
            if (rb == null)
            {
                return;
            }

            ConfigureRigidbody();
            StopRigidbodyMotion();

            rb.WakeUp();
        }
        private void StopRigidbodyMotion()
        {
            if (rb == null)
            {
                return;
            }

            if (rb.isKinematic)
            {
                return;
            }

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        private void PlayWalkAnimation()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (animator == null)
            {
                Debug.LogWarning($"EnemyController on {gameObject.name}: No Animator found in children.");
                return;
            }

            animator.enabled = true;
            animator.Rebind();
            animator.Update(0f);

            if (!string.IsNullOrEmpty(deathTrigger))
            {
                animator.ResetTrigger(deathTrigger);
            }

            if (!string.IsNullOrEmpty(walkStateName))
            {
                animator.Play(walkStateName, 0, 0f);
            }
        }

        private void FixedUpdate()
        {
            if (!isInitialized)
            {
                return;
            }

            if (currentState != EnemyState.Moving)
            {
                return;
            }

            if (rb == null)
            {
                return;
            }

            if (continueStraightAfterReachingPlayer &&
                !hasReachedPlayer &&
                HasReachedOrPassedPlayer())
            {
                hasReachedPlayer = true;
            }

            if (retargetPlayerWhileMoving && !hasReachedPlayer)
            {
                retargetTimer -= Time.fixedDeltaTime;

                if (retargetTimer <= 0f)
                {
                    CalculateMoveDirection(false);
                    retargetTimer = Mathf.Max(0.02f, retargetInterval);
                }
            }

            Vector3 newPosition =
                rb.position +
                moveDirection * currentSpeed * Time.fixedDeltaTime;

            if (TryGetGroundPosition(newPosition, out Vector3 groundedPosition))
            {
                newPosition = groundedPosition;
            }

            rb.MovePosition(newPosition);
        }

        private bool HasReachedOrPassedPlayer()
        {
            if (!TryGetPlayerTargetPosition(out Vector3 playerPosition))
            {
                return false;
            }

            Vector3 currentPosition = rb != null ? rb.position : transform.position;

            playerPosition.y = currentPosition.y;

            Vector3 toPlayer = playerPosition - currentPosition;
            toPlayer.y = 0f;

            float distanceToPlayer = toPlayer.magnitude;

            if (distanceToPlayer <= stopRetargetingDistance)
            {
                return true;
            }

            if (moveDirection.sqrMagnitude > 0.001f &&
                toPlayer.sqrMagnitude > 0.001f &&
                Vector3.Dot(toPlayer.normalized, moveDirection.normalized) < -0.1f)
            {
                return true;
            }

            return false;
        }

        private bool TryGetGroundPosition(Vector3 position, out Vector3 groundPosition)
        {
            Vector3 rayOrigin = position + Vector3.up * groundRayHeight;
            float rayDistance = groundRayHeight + groundRayDistance;

            if (Physics.Raycast(
                rayOrigin,
                Vector3.down,
                out RaycastHit hit,
                rayDistance,
                groundLayer,
                QueryTriggerInteraction.Ignore))
            {
                groundPosition = position;
                groundPosition.y = hit.point.y + groundOffset;
                return true;
            }

            groundPosition = position;
            return false;
        }

        private void SnapToGround()
        {
            if (rb == null)
            {
                return;
            }

            if (TryGetGroundPosition(rb.position, out Vector3 groundedPosition))
            {
                rb.position = groundedPosition;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isInitialized)
            {
                return;
            }

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

            if (deathRoutine != null)
            {
                StopCoroutine(deathRoutine);
                deathRoutine = null;
            }

            isInitialized = false;
            currentState = EnemyState.Idle;

            StopRigidbodyMotion();
        }

        public void Despawn(bool killedByPlayer)
        {
            if (!isInitialized)
            {
                return;
            }

            if (currentState == EnemyState.Dead)
            {
                return;
            }

            isInitialized = false;
            currentState = EnemyState.Dead;
            wasKilledByPlayer = killedByPlayer;

            StopRigidbodyMotion();

            EventManager.Trigger(new EnemyDespawnedEvent
            {
                enemy = gameObject,
                wasKilledByPlayer = killedByPlayer
            });

            if (!killedByPlayer)
            {
                ReturnToPoolOrDestroy();
                return;
            }

            SetCollidersEnabled(false);
            PlayDeathAnimation();
            PlayDeathSound();

            if (rb != null)
            {
                rb.isKinematic = true;
            }

            deathRoutine = StartCoroutine(ReturnAfterDeathDelay());
        }

        private IEnumerator ReturnAfterDeathDelay()
        {
            yield return new WaitForSeconds(deathReturnDelay);

            deathRoutine = null;
            ReturnToPoolOrDestroy();
        }

        private void ReturnToPoolOrDestroy()
        {
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

        private void SetCollidersEnabled(bool isEnabled)
        {
            if (colliders == null || colliders.Length == 0)
            {
                colliders = GetComponentsInChildren<Collider>();
            }

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = isEnabled;
                }
            }
        }

        private void PlayDeathAnimation()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (animator != null && !string.IsNullOrEmpty(deathTrigger))
            {
                animator.SetTrigger(deathTrigger);
            }
        }

        private void PlayDeathSound()
        {
            AudioClip clipToPlay = deathSound;

            if (clipToPlay == null && data != null)
            {
                clipToPlay = data.deathSFX;
            }

            if (clipToPlay == null)
            {
                return;
            }

            if (ActiveSagaAudioManager.Instance != null)
            {
                ActiveSagaAudioManager.Instance.PlaySFX(clipToPlay);
                return;
            }

            AudioSource.PlayClipAtPoint(clipToPlay, transform.position);
        }
    }
}