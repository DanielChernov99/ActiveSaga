using UnityEngine;
using ActiveSaga.BossFight.Entities;

namespace ActiveSaga.BossFight.Core
{
    public class DespawnBoundary : MonoBehaviour
    {
        [Header("Damage")]
        [SerializeField] private float enemyDamage = 10f;

        private void OnTriggerEnter(Collider other)
        {
            // Enemy reached the back wall
            var enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                if (BossFightGameManager.Instance != null)
                {
                    BossFightGameManager.Instance.TakeDamage(enemyDamage);
                }

                enemy.Despawn(false);
                return;
            }

            // Projectile reached the back wall
            var proj = other.GetComponent<ProjectileController>();
            if (proj != null)
            {
                proj.Despawn();
                return;
            }
        }
    }
}