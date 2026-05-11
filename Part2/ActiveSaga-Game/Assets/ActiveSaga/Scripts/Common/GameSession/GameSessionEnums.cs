namespace ActiveSaga.Common.GameSession
{
    public enum GameType
    {
        None = 0,
        RunGame = 1,
        FightGame = 2
    }

    public enum GameEndReason
    {
        None = 0,
        GameOver = 1,
        GameWon = 2,
        PlayerQuit = 3
    }

    public enum GameSessionState
    {
        NotStarted = 0,
        Running = 1,
        Paused = 2,
        WaitingForServer = 3,
        Ended = 4
    }
}