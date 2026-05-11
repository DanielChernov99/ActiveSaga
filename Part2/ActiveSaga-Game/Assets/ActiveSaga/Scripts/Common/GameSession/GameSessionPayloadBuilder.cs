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

            AppendString(sb, "sessionId", sessionId);
            sb.Append(",");

            AppendString(sb, "startedUtc", startedUtc);
            sb.Append(",");

            AppendString(sb, "endedUtc", endedUtc);
            sb.Append(",");

            AppendString(sb, "gameType", gameType.ToString());
            sb.Append(",");

            AppendString(sb, "endReason", endReason.ToString());
            sb.Append(",");

            AppendNumber(sb, "durationSeconds", durationSeconds);
            sb.Append(",");

            sb.Append("\"gameStats\":");
            sb.Append(statsJson);

            sb.Append("}");

            return sb.ToString();
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