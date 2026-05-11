namespace ActiveSaga.Common.GameSession
{
    public abstract class GameStatsSnapshot
    {
        public abstract GameType GameType { get; }

        public abstract string ToJson();
    }
}