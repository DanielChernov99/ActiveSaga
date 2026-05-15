using UnityEngine;
using ActiveSaga.Common.GameSession;

namespace ActiveSaga.RunGame
{
    public class RunGameStatsTracker : GameStatsTracker
    {
        private float distanceMeters;
        private int enemiesKilled;

        private int totalJumps;
        private int totalSquats;

        private int obstacleCrashes;
        private int obstacleGrazes;
        private int coinsCollected;
        public override GameType GameType => GameType.RunGame;

        public float DistanceMeters => distanceMeters;
        public int EnemiesKilled => enemiesKilled;
        public int TotalJumps => totalJumps;
        public int TotalSquats => totalSquats;
        public int ObstacleCrashes => obstacleCrashes;
        public int ObstacleGrazes => obstacleGrazes;
        public int CoinsCollected => coinsCollected;



        public override void ResetStats()
        {
            distanceMeters = 0f;
            enemiesKilled = 0;

            totalJumps = 0;
            totalSquats = 0;

            obstacleCrashes = 0;
            obstacleGrazes = 0;
            coinsCollected = 0;
        }

        public void SetDistance(float totalDistanceMeters)
        {
            distanceMeters = Mathf.Max(0f, totalDistanceMeters);
        }

        public void AddDistance(float meters)
        {
            if (meters <= 0f)
            {
                return;
            }

            distanceMeters += meters;
        }

        public void AddEnemyKill()
        {
            enemiesKilled++;
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            coinsCollected += amount;
        }

        public void AddJump()
        {
            totalJumps++;
        }

        public void AddSquat()
        {
            totalSquats++;
        }

        public void AddObstacleCrash()
        {
            obstacleCrashes++;
        }

        public void AddObstacleGraze()
        {
            obstacleGrazes++;
        }

        public override GameStatsSnapshot BuildSnapshot()
        {
            return new RunGameStatsSnapshot
            {
                distanceMeters = distanceMeters,
                enemiesKilled = enemiesKilled,
                totalJumps = totalJumps,
                totalSquats = totalSquats,
                obstacleCrashes = obstacleCrashes,
                obstacleGrazes = obstacleGrazes
            };
        }

        
    }
}