using UnityEngine;

namespace ActiveSaga.BossFight.Data
{
    public enum ProjectilePattern { Linear, Sine }

    [CreateAssetMenu(fileName = "NewProjectileData", menuName = "BossFight/ProjectileData")]
    public class ProjectileData : ScriptableObject
    {
        public string projectileName;
        public GameObject prefab;
        public float speed = 10f;
        public float damage = 10f;
        public float lifetime = 5f;

        [Header("Patterns")]
        public ProjectilePattern pattern = ProjectilePattern.Linear;
        public float amplitude = 1f;
        public float frequency = 2f;

        [Header("Visuals")]
        public GameObject impactVFX;
        public AudioClip launchSFX;
    }
}
