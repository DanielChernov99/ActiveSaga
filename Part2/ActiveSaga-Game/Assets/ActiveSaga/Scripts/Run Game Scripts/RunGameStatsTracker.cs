using UnityEngine;
using ActiveSaga.Common.GameSession;

namespace ActiveSaga.RunGame
{
    public class RunGameStatsTracker : GameStatsTracker
    {
        private float distanceMeters;
        private int enemiesKilled;

        public override GameType GameType => GameType.RunGame;

        public float DistanceMeters => distanceMeters;
        public int EnemiesKilled => enemiesKilled;

        public override void ResetStats()
        {
            distanceMeters = 0f;
            enemiesKilled = 0;
        }

        public void AddDistance(float meters)
        {
            if (meters <= 0f)
            {
                return;
            }

            distanceMeters += meters;
        }

        public void SetDistance(float totalDistanceMeters)
        {
            distanceMeters = Mathf.Max(0f, totalDistanceMeters);
        }

        public void AddEnemyKill()
        {
            enemiesKilled++;
        }

        public override GameStatsSnapshot BuildSnapshot()
        {
            return new RunGameStatsSnapshot
            {
                distanceMeters = distanceMeters,
                enemiesKilled = enemiesKilled
            };
        }
    }
}