using System.Globalization;
using ActiveSaga.Common.GameSession;

namespace ActiveSaga.RunGame
{
    public class RunGameStatsSnapshot : GameStatsSnapshot
    {
        public float distanceMeters;
        public int enemiesKilled;

        public override GameType GameType => GameType.RunGame;

        public override string ToJson()
        {
            string distance = distanceMeters.ToString("0.###", CultureInfo.InvariantCulture);

            return "{"
                   + "\"distanceMeters\":" + distance + ","
                   + "\"enemiesKilled\":" + enemiesKilled
                   + "}";
        }
    }
}