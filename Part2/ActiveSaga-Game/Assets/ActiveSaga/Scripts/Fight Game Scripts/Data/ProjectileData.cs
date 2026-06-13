using UnityEngine;

namespace ActiveSaga.BossFight.Data
{
    public enum ProjectilePattern { Linear, Sine }

    public enum ProjectileDodgeAction
    {
        Random,
        Jump,
        Duck
    }

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

        [Header("Dodge Setup")]
        [Tooltip("Jump = low obstacle. Duck = high obstacle. Random is useful only for generic test assets.")]
        public ProjectileDodgeAction requiredDodgeAction = ProjectileDodgeAction.Random;

        [Tooltip("Height above the player's floor for a low obstacle that should be jumped over.")]
        [Min(0.05f)]
        public float jumpObstacleHeight = 0.55f;

        [Tooltip("Height above the player's floor for a high obstacle that should be ducked under.")]
        [Min(0.05f)]
        public float duckObstacleHeight = 1.35f;

        [Header("Movement Pattern")]
        public ProjectilePattern pattern = ProjectilePattern.Linear;

        [Tooltip("Only used for Sine pattern")]
        public float amplitude = 1f;

        [Tooltip("Only used for Sine pattern")]
        public float frequency = 2f;

        [Header("VFX / SFX")]
        public GameObject impactVFX;
        public AudioClip launchSFX;

        [Header("Hit Feedback")]
        public AudioClip hitPlayerSFX;

        [Range(0f, 1f)]
        public float hitPlayerSFXVolume = 1f;

        public ProjectileDodgeAction ResolveDodgeAction()
        {
            if (requiredDodgeAction == ProjectileDodgeAction.Random)
            {
                return Random.value > 0.5f
                    ? ProjectileDodgeAction.Duck
                    : ProjectileDodgeAction.Jump;
            }

            return requiredDodgeAction;
        }

        public float GetTargetHeight(ProjectileDodgeAction action)
        {
            if (action == ProjectileDodgeAction.Duck)
            {
                return duckObstacleHeight;
            }

            return jumpObstacleHeight;
        }

        private void OnValidate()
        {
            if (speed < 0f) speed = 0f;
            if (lifetime < 0.1f) lifetime = 0.1f;
            if (frequency < 0f) frequency = 0f;
            if (amplitude < 0f) amplitude = 0f;
            if (jumpObstacleHeight < 0.05f) jumpObstacleHeight = 0.05f;
            if (duckObstacleHeight < 0.05f) duckObstacleHeight = 0.05f;

            if (string.IsNullOrWhiteSpace(projectileName))
            {
                projectileName = name;
            }
        }
    }
}