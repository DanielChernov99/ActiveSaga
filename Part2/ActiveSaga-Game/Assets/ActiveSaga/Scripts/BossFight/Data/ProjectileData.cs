using UnityEngine;

namespace ActiveSaga.BossFight.Data
{
    public enum ProjectilePattern { Linear, Sine }

    [CreateAssetMenu(fileName = "NewProjectileData", menuName = "BossFight/ProjectileData")]
    public class ProjectileData : ScriptableObject
    {
        [Header("Identity")]
        public string projectileName;

        [Tooltip("Prefab used by PoolManager")]
        public GameObject prefab;

        [Header("Stats")]
        [Min(0f)]
        public float speed = 10f;

        [Min(0f)]
        public float damage = 10f;

        [Min(0.1f)]
        public float lifetime = 5f;

        [Header("Movement Pattern")]
        public ProjectilePattern pattern = ProjectilePattern.Linear;

        [Tooltip("Only used for Sine pattern")]
        public float amplitude = 1f;

        [Tooltip("Only used for Sine pattern")]
        public float frequency = 2f;

        [Header("VFX / SFX")]
        public GameObject impactVFX;
        public AudioClip launchSFX;

        private void OnValidate()
        {
            // Safety checks in Editor
            if (speed < 0) speed = 0;
            if (lifetime < 0.1f) lifetime = 0.1f;
            if (frequency < 0) frequency = 0;
            if (amplitude < 0) amplitude = 0;

            if (string.IsNullOrWhiteSpace(projectileName))
            {
                projectileName = name;
            }
        }
    }
}