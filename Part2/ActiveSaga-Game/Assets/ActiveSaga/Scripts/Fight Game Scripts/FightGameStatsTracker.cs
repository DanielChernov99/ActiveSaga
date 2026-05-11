using ActiveSaga.Common.GameSession;

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