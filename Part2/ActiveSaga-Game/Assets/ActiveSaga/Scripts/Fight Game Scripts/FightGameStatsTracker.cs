using ActiveSaga.Common.GameSession;
using ActiveSaga.BossFight.Core;

namespace ActiveSaga.FightGame
{
    public class FightGameStatsTracker : GameStatsTracker
    {
        private int wavesCompleted;
        private int enemiesKilled;
        private int successfulDodges;

        public override GameType GameType => GameType.FightGame;

        public int WavesCompleted => wavesCompleted;
        public int EnemiesKilled => enemiesKilled;
        public int SuccessfulDodges => successfulDodges;

        private void OnEnable()
        {
            EventManager.Subscribe<WaveCompletedEvent>(OnWaveCompleted);
            EventManager.Subscribe<EnemyDespawnedEvent>(OnEnemyDespawned);
            EventManager.Subscribe<ProjectileDespawnedEvent>(OnProjectileDespawned);
        }

        private void OnDisable()
        {
            EventManager.Unsubscribe<WaveCompletedEvent>(OnWaveCompleted);
            EventManager.Unsubscribe<EnemyDespawnedEvent>(OnEnemyDespawned);
            EventManager.Unsubscribe<ProjectileDespawnedEvent>(OnProjectileDespawned);
        }

        public override void ResetStats()
        {
            wavesCompleted = 0;
            enemiesKilled = 0;
            successfulDodges = 0;
        }

        public void AddWaveCompleted()
        {
            wavesCompleted++;
        }

        public void AddEnemyKill()
        {
            enemiesKilled++;
        }

        public void AddSuccessfulDodge()
        {
            successfulDodges++;
        }

        private void OnWaveCompleted(WaveCompletedEvent e)
        {
            if (e.success)
            {
                AddWaveCompleted();
            }
        }

        private void OnEnemyDespawned(EnemyDespawnedEvent e)
        {
            if (e.wasKilledByPlayer)
            {
                AddEnemyKill();
            }
        }

        private void OnProjectileDespawned(ProjectileDespawnedEvent e)
        {
            if (e.wasDodged)
            {
                AddSuccessfulDodge();
            }
        }

        public override GameStatsSnapshot BuildSnapshot()
        {
            return new FightGameStatsSnapshot
            {
                wavesCompleted = wavesCompleted,
                enemiesKilled = enemiesKilled,
                successfulDodges = successfulDodges
            };
        }
    }
}
