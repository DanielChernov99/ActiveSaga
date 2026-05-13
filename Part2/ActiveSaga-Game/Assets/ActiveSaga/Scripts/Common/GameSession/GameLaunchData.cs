

namespace ActiveSaga.Common.GameSession
{
    public enum SelectedGameMode
    {
        None,
        Run,
        Fight
    }

    public enum SelectedGameDifficulty
    {
        None,
        Easy,
        Medium,
        Hard
    }

    public static class GameLaunchData
    {
        public static SelectedGameMode GameMode { get; private set; } = SelectedGameMode.None;
        public static SelectedGameDifficulty Difficulty { get; private set; } = SelectedGameDifficulty.None;

        public static void SetSelection(SelectedGameMode gameMode, SelectedGameDifficulty difficulty)
        {
            GameMode = gameMode;
            Difficulty = difficulty;
        }

        public static void Clear()
        {
            GameMode = SelectedGameMode.None;
            Difficulty = SelectedGameDifficulty.None;
        }

        public static string GetServerGameType()
        {
            if (GameMode == SelectedGameMode.Run)
            {
                return "RUN";
            }

            if (GameMode == SelectedGameMode.Fight)
            {
                return "FIGHT";
            }

            return "";
        }

        public static int GetDifficultyNumber()
        {
            if (Difficulty == SelectedGameDifficulty.Easy)
            {
                return 1;
            }

            if (Difficulty == SelectedGameDifficulty.Medium)
            {
                return 2;
            }

            if (Difficulty == SelectedGameDifficulty.Hard)
            {
                return 3;
            }

            return 0;
        }
    }
}