using System;
using System.Collections;
using ActiveSaga.MainScreen.Data;
using UnityEngine;
using UnityEngine.Networking;

namespace ActiveSaga.Common.Networking
{
    public class PlayerApiService : MonoBehaviour
    {
        public const string AuthTokenKey = "ACTIVE_SAGA_AUTH_TOKEN";

        [Header("API")]
        [SerializeField] private string playerApiBaseUrl = "http://localhost:3000/api/player";

        public static void SaveToken(string token)
        {
            PlayerPrefs.SetString(AuthTokenKey, token);
            PlayerPrefs.Save();
        }

        public static string GetToken()
        {
            return PlayerPrefs.GetString(AuthTokenKey, "");
        }

        public static bool HasToken()
        {
            return !string.IsNullOrWhiteSpace(GetToken());
        }

        public static void ClearToken()
        {
            PlayerPrefs.DeleteKey(AuthTokenKey);
            PlayerPrefs.Save();
        }

        public IEnumerator GetPlayerStats(
            Action<PlayerStatsResponse> onSuccess,
            Action<string> onError)
        {
            string url = BuildUrl("me");

            yield return SendGetRequest(
                url,
                json =>
                {
                    PlayerStatsResponse response = JsonUtility.FromJson<PlayerStatsResponse>(json);
                    onSuccess?.Invoke(response);
                },
                onError
            );
        }

        public IEnumerator GetDailyQuests(
            Action<DailyQuestsResponse> onSuccess,
            Action<string> onError)
        {
            string url = BuildUrl("daily-quests");

            yield return SendGetRequest(
                url,
                json =>
                {
                    DailyQuestsResponse response = JsonUtility.FromJson<DailyQuestsResponse>(json);
                    onSuccess?.Invoke(response);
                },
                onError
            );
        }

        public IEnumerator GetDailyStreak(
            Action<DailyStreakResponse> onSuccess,
            Action<string> onError)
        {
            string url = BuildUrl("daily-streak");

            yield return SendGetRequest(
                url,
                json =>
                {
                    DailyStreakResponse response = JsonUtility.FromJson<DailyStreakResponse>(json);
                    onSuccess?.Invoke(response);
                },
                onError
            );
        }
        private string BuildUrl(string path)
        {
            string cleanBase = playerApiBaseUrl.TrimEnd('/');
            string cleanPath = path.TrimStart('/');

            return cleanBase + "/" + cleanPath;
        }

        private IEnumerator SendGetRequest(
            string url,
            Action<string> onSuccess,
            Action<string> onError)
        {
            string token = GetToken();

            if (string.IsNullOrWhiteSpace(token))
            {
                onError?.Invoke("Missing auth token. Login again.");
                yield break;
            }

            using UnityWebRequest request = UnityWebRequest.Get(url);

            request.SetRequestHeader("Authorization", "Bearer " + token);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            bool hasError =
                request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError ||
                request.result == UnityWebRequest.Result.DataProcessingError;

            if (hasError)
            {
                string error =
                    "URL: " + url +
                    "\nCode: " + request.responseCode +
                    "\nError: " + request.error +
                    "\nBody: " + request.downloadHandler.text;

                onError?.Invoke(error);
                yield break;
            }

            onSuccess?.Invoke(request.downloadHandler.text);
        }
    }
}