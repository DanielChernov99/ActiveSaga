using ActiveSaga.Common.GameSession;
using ActiveSaga.Common.UI;

namespace ActiveSaga.RunGame
{
    public class RunGameResultsUI : EndGameResultsViewBase
    {
        protected override string BuildGameSpecificStatsText(GameStatsSnapshot statsSnapshot)
        {
            RunGameStatsSnapshot runStats = statsSnapshot as RunGameStatsSnapshot;

            if (runStats == null)
            {
                return "Run game stats missing.";
            }

            return
                "Distance: " + runStats.distanceMeters.ToString("0") + " m" +
                "\nEnemies Killed: " + runStats.enemiesKilled +
                "\nJumps: " + runStats.totalJumps +
                "\nSquats: " + runStats.totalSquats +
                "\nObstacle Crashes: " + runStats.obstacleCrashes;

        }
    }
}