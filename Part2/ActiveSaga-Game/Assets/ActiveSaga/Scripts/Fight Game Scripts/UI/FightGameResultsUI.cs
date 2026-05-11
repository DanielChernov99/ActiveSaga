using ActiveSaga.Common.GameSession;
using ActiveSaga.Common.UI;

namespace ActiveSaga.FightGame
{
    public class FightGameResultsUI : EndGameResultsViewBase
    {
        protected override string BuildGameSpecificStatsText(GameStatsSnapshot statsSnapshot)
        {
            FightGameStatsSnapshot fightStats = statsSnapshot as FightGameStatsSnapshot;

            if (fightStats == null)
            {
                return "Fight game stats missing.";
            }

            return
                "Waves Completed: " + fightStats.wavesCompleted +
                "\nEnemies Killed: " + fightStats.enemiesKilled +
                "\nSuccessful Dodges: " + fightStats.successfulDodges;
        }
    }
}