using ActiveSaga.Common.GameSession;
using ActiveSaga.Common.UI;

namespace ActiveSaga.FightGame
{
    public class FightGameResultsUI : EndGameResultsViewBase
    {
        protected override void BuildGameSpecificStats(
            GameStatsSnapshot statsSnapshot,
            out ResultStat stat1,
            out ResultStat stat2,
            out ResultStat stat3
        )
        {
            FightGameStatsSnapshot fightStats = statsSnapshot as FightGameStatsSnapshot;

            if (fightStats == null)
            {
                stat1 = new ResultStat("WAVES COMPLETED", "-");
                stat2 = new ResultStat("ENEMIES KILLED", "-");
                stat3 = new ResultStat("SUCCESSFUL DODGES", "-");
                return;
            }

            stat1 = new ResultStat("WAVES COMPLETED", fightStats.wavesCompleted.ToString());
            stat2 = new ResultStat("ENEMIES KILLED", fightStats.enemiesKilled.ToString());
            stat3 = new ResultStat("SUCCESSFUL DODGES", fightStats.successfulDodges.ToString());
        }
    }
}