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

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = false; 
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        public void Initialize(ProjectileData projectileData)
        {
            this.data = projectileData;
            isInitialized = true;

            // Target the headset (Camera) specifically
            Transform target = BossFightGameManager.Instance.PlayerCamera != null ? 
                BossFightGameManager.Instance.PlayerCamera.transform : 
                BossFightGameManager.Instance.PlayerTransform;

            if (target != null)
            {
                transform.LookAt(target.position);
                rb.linearVelocity = transform.forward * data.speed;
}
            else
            {
                Debug.LogError("ProjectileController: No target found for initialization!");
            }

            EventManager.Trigger(new ProjectileSpawnedEvent { projectile = gameObject });
            
            // Return to pool after lifetime
            CancelInvoke(nameof(Despawn));
            Invoke(nameof(Despawn), data.lifetime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isInitialized) return;

            // Check if hit player (XR Headset or Body Collider)
            if (other.CompareTag("PlayerHitbox") || other.CompareTag("MainCamera") || other.CompareTag("Player"))
            {
                Debug.Log($"<color=orange>Projectile Hit Player: {data.projectileName}</color>");
                BossFightGameManager.Instance.TakeDamage(data.damage);
                Despawn();
            }
            // Check if deflected
            else if (other.CompareTag("Sword") || other.CompareTag("DodgeShield"))
            {
                Debug.Log($"Projectile {data.projectileName} deflected.");
                Despawn();
            }
        }

        private void Despawn()
        {
            if (!isInitialized) return;
            isInitialized = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            EventManager.Trigger(new ProjectileDespawnedEvent { projectile = gameObject });
            PoolManager.Instance.ReturnToPool(gameObject, data.projectileName);
        }
    }
}


