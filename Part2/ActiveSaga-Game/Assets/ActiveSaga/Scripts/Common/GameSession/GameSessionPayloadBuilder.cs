using System.Globalization;
using System.Text;

namespace ActiveSaga.Common.GameSession
{
    public static class GameSessionPayloadBuilder
    {
        public static string BuildJson(
            string sessionId,
            string startedUtc,
            string endedUtc,
            GameType gameType,
            GameEndReason endReason,
            float durationSeconds,
            GameStatsSnapshot statsSnapshot)
        {
            string statsJson = statsSnapshot != null ? statsSnapshot.ToJson() : "{}";

            StringBuilder sb = new StringBuilder();

            sb.Append("{");

            AppendString(sb, "clientSessionId", sessionId);
            sb.Append(",");

            AppendString(sb, "startedUtc", startedUtc);
            sb.Append(",");

            AppendString(sb, "endedUtc", endedUtc);
            sb.Append(",");

            AppendString(sb, "gameType", ConvertGameTypeForServer(gameType));
            sb.Append(",");

            AppendString(sb, "endReason", ConvertEndReasonForServer(endReason));
            sb.Append(",");

            AppendNumber(sb, "durationSeconds", durationSeconds);
            sb.Append(",");

            sb.Append("\"stats\":");
            sb.Append(statsJson);

            sb.Append("}");

            return sb.ToString();
        }

        private static string ConvertGameTypeForServer(GameType gameType)
        {
            switch (gameType)
            {
                case GameType.RunGame:
                    return "RUN";

                case GameType.FightGame:
                    return "FIGHT";

                default:
                    return gameType.ToString().ToUpperInvariant();
            }
        }

        private static string ConvertEndReasonForServer(GameEndReason endReason)
        {
            switch (endReason)
            {
                case GameEndReason.GameOver:
                    return "GAME_OVER";

                case GameEndReason.GameWon:
                    return "GAME_WON";

                case GameEndReason.PlayerQuit:
                    return "QUIT";

                default:
                    return "UNKNOWN";
            }
        }

        private static void AppendString(StringBuilder sb, string key, string value)
        {
            sb.Append("\"");
            sb.Append(Escape(key));
            sb.Append("\":\"");
            sb.Append(Escape(value));
            sb.Append("\"");
        }

        private static void AppendNumber(StringBuilder sb, string key, float value)
        {
            sb.Append("\"");
            sb.Append(Escape(key));
            sb.Append("\":");
            sb.Append(value.ToString("0.###", CultureInfo.InvariantCulture));
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }

            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");
        }
    }
}