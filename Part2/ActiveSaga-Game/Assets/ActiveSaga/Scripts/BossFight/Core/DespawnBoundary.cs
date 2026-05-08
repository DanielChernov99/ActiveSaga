using UnityEngine;
using ActiveSaga.BossFight.Entities;

namespace ActiveSaga.BossFight.Core
{
    public class DespawnBoundary : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            var enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.Despawn(false);
                return;
            }

            var proj = other.GetComponent<ProjectileController>();
            if (proj != null)
            {
                proj.Despawn();
                return;
            }
        }
    }
}