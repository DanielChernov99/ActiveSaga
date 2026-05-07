using UnityEngine;

namespace ActiveSaga.BossFight.Data
{
    [CreateAssetMenu(fileName = "NewProjectileData", menuName = "BossFight/ProjectileData")]
    public class ProjectileData : ScriptableObject
    {
        public string projectileName;
        public GameObject prefab;
        public float speed = 10f;
        public float damage = 10f;
        public float lifetime = 5f;

        [Header("Visuals")]
        public GameObject impactVFX;
        public AudioClip launchSFX;
    }
}
