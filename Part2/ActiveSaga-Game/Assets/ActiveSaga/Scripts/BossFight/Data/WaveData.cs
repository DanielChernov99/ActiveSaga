using UnityEngine;
using System.Collections.Generic;

namespace ActiveSaga.BossFight.Data
{
    [System.Serializable]
    public class WaveStep
    {
        public enum StepType { SpawnEnemy, SpawnProjectile, Delay, BossAnimation }
        
        public StepType type;
        public EnemyData enemyData;
        public ProjectileData projectileData;
        public string animationTrigger;
        
        public float delayAfterStep = 1f;
        public Vector3 spawnOffset;
    }

    [CreateAssetMenu(fileName = "NewWaveData", menuName = "BossFight/WaveData")]
    public class WaveData : ScriptableObject
    {
        public string waveName;
        public List<WaveStep> steps;
        public float difficultyMultiplier = 1.0f;
    }
}
