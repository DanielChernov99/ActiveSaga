using ActiveSaga.Common.GameSession;

namespace ActiveSaga.FightGame
{
    public class FightGameStatsSnapshot : GameStatsSnapshot
    {
        public int wavesCompleted;
        public int enemiesKilled;
        public int successfulDodges;
        public float bossDamageDealt;

        public override GameType GameType => GameType.FightGame;

        public override string ToJson()
        {
            return "{"
                   + "\"wavesCompleted\":" + wavesCompleted + ","
                   + "\"enemiesKilled\":" + enemiesKilled + ","
                   + "\"dodges\":" + successfulDodges + ","
                   + "\"bossDamageDealt\":" + bossDamageDealt
                   + "}";
        }
    }
}