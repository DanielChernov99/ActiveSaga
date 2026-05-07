using UnityEngine;
using ActiveSaga.BossFight.Core;
using ActiveSaga.BossFight.Data;
using ActiveSaga.BossFight.Combat;

namespace ActiveSaga.BossFight.Entities
{
    [RequireComponent(typeof(Rigidbody))]
    public class EnemyController : MonoBehaviour
    {
        public enum EnemyState { Idle, Spawning, Moving, AttackWindow, Hit, Dead }

        [Header("State")]
        [SerializeField] private EnemyState currentState = EnemyState.Idle;
        
        private EnemyData data;
        private Rigidbody rb;
        private bool isInitialized = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true; // Movement via MovePosition
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        public void Initialize(EnemyData enemyData)
        {
            this.data = enemyData;
            currentState = EnemyState.Spawning;
            isInitialized = true;

            // Visual Setup
            var renderer = GetComponentInChildren<Renderer>();
            if (renderer != null) renderer.material.color = data.enemyColor;

            EventManager.Trigger(new EnemySpawnedEvent { enemy = gameObject });
            
            currentState = EnemyState.Moving;
            Debug.Log($"Enemy Initialized: {data.enemyName} at {transform.position}");
        }

        private void FixedUpdate()
        {
            if (!isInitialized || currentState != EnemyState.Moving) return;

            // Target the headset's position on the ground plane
            Vector3 targetPos = BossFightGameManager.Instance.PlayerCamera != null ? 
                BossFightGameManager.Instance.PlayerCamera.transform.position : 
                BossFightGameManager.Instance.PlayerTransform.position;
            
            targetPos.y = transform.position.y; // Keep on floor plane

            float dist = Vector3.Distance(rb.position, targetPos);
            
            // Move towards player
            Vector3 nextPos = Vector3.MoveTowards(rb.position, targetPos, data.moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(nextPos);

            // Look at player
            if (dist > 0.1f)
            {
                transform.LookAt(targetPos);
            }

            // Debug log every second or so
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"Enemy {data.enemyName} Pos: {rb.position}, Target: {targetPos}, Distance: {dist:F2}");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (currentState == EnemyState.Dead || currentState == EnemyState.Hit) return;

            // Check for weapon hit
            WeaponController weapon = other.GetComponent<WeaponController>();
            if (weapon == null) weapon = other.GetComponentInParent<WeaponController>();

            if (weapon != null)
            {
                ValidateHit(weapon);
                return;
            }

            // Check if reached player body or headset
            if (other.CompareTag("PlayerHitbox") || other.CompareTag("MainCamera") || other.CompareTag("Player"))
            {
                ReachPlayer();
            }
        }

        private void ValidateHit(WeaponController weapon)
        {
            if (data.requiredHand != HandType.Any && weapon.Hand != data.requiredHand) return;
            if (weapon.Velocity.magnitude < data.velocityThreshold) return;

            HandleDeath(true);
        }

        private void ReachPlayer()
        {
            Debug.Log($"<color=red>Enemy {data.enemyName} reached player!</color>");
            BossFightGameManager.Instance.TakeDamage(10f); 
            HandleDeath(false);
        }

        private void HandleDeath(bool wasSuccess)
        {
            if (currentState == EnemyState.Dead) return;
            currentState = EnemyState.Dead;
            
            if (wasSuccess)
            {
                if (data.deathVFX != null) Instantiate(data.deathVFX, transform.position, Quaternion.identity);
            }

            EventManager.Trigger(new EnemyDespawnedEvent { enemy = gameObject });
            PoolManager.Instance.ReturnToPool(gameObject, data.enemyName);
        }

        private void OnDrawGizmos()
        {
            if (!isInitialized) return;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);
        }
    }
}


