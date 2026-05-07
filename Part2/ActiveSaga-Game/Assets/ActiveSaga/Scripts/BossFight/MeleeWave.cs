using UnityEngine;

namespace ActiveSaga.BossFight
{
    public class MeleeWave : BossWave
    {
        [SerializeField] private EnemySpawner spawner;

        public override void StartWave(float difficultyMultiplier)
        {
            if (spawner == null) spawner = FindObjectOfType<EnemySpawner>();
            spawner.StartWave(difficultyMultiplier);
        }

        public override void EndWave()
        {
            Debug.Log("Melee Wave Logic Ended.");
        }
    }
}
