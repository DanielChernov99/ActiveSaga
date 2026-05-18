using System.Globalization;
using ActiveSaga.Common.GameSession;

namespace ActiveSaga.RunGame
{
    public class RunGameStatsSnapshot : GameStatsSnapshot
    {
        public float distanceMeters;
        public int enemiesKilled;
        public int coinsCollected;

        public int totalJumps;
        public int totalSquats;
        

        public int obstacleCrashes;
        public int obstacleGrazes;

        public override GameType GameType => GameType.RunGame;

        public override string ToJson()
        {
            string distance = distanceMeters.ToString("0.###", CultureInfo.InvariantCulture);

            return "{"
                   + "\"distanceMeters\":" + distance + ","
                   + "\"enemiesKilled\":" + enemiesKilled + ","
                   + "\"totalJumps\":" + totalJumps + ","
                   + "\"totalSquats\":" + totalSquats + ","
                   + "\"obstacleCrashes\":" + obstacleCrashes + ","
                   + "\"obstacleGrazes\":" + obstacleGrazes + ","
                   + "\"coinsCollected\":" + coinsCollected
                   + "}";
        }
    }
}