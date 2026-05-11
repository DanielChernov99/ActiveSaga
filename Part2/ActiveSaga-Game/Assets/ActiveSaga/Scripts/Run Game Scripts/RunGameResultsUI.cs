using ActiveSaga.Common.GameSession;
using ActiveSaga.Common.UI;

namespace ActiveSaga.RunGame
{
    public class RunGameResultsUI : EndGameResultsViewBase
    {
        protected override void BuildGameSpecificStats(
            GameStatsSnapshot statsSnapshot,
            out ResultStat stat1,
            out ResultStat stat2,
            out ResultStat stat3
        )
        {
            RunGameStatsSnapshot runStats = statsSnapshot as RunGameStatsSnapshot;

            if (runStats == null)
            {
                stat1 = new ResultStat("DISTANCE", "-");
                stat2 = new ResultStat("ENEMIES KILLED", "-");
                stat3 = new ResultStat("JUMPS", "-");
                return;
            }

            stat1 = new ResultStat("DISTANCE", runStats.distanceMeters.ToString("0") + " m");
            stat2 = new ResultStat("ENEMIES KILLED", runStats.enemiesKilled.ToString());
            stat3 = new ResultStat("JUMPS", runStats.totalJumps.ToString());
        }
    }
}