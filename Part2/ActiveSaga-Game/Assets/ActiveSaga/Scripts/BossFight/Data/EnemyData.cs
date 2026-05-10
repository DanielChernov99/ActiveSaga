using UnityEngine;

namespace ActiveSaga.BossFight.Data
{
    public enum HandType { Any, Left, Right }
    public enum HitDirection { Any, Up, Down, Left, Right }

    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "BossFight/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        public string enemyName;
        public GameObject prefab;
        public Color enemyColor = Color.white;
        
        [Header("Stats")]
        public float health = 1f;
        public float moveSpeed = 5f;
        public float scoreValue = 100f;

        [Header("Requirements")]
        public HandType requiredHand = HandType.Any;
        public HitDirection requiredDirection = HitDirection.Any;
        public float velocityThreshold = 1.0f;

        [Header("Visuals/Audio")]
        public GameObject deathVFX;
        public AudioClip deathSFX;
        public AudioClip spawnSFX;
    }
}
