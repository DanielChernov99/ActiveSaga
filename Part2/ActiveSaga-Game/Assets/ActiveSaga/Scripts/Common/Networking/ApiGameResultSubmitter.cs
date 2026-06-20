using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ActiveSaga.Common.Networking
{
    public class ApiGameResultSubmitter : MonoBehaviour, IGameResultSubmitter
    {
        [Header("API Settings")]
        [SerializeField] private string baseUrl = "https://active-saga-api.onrender.com";
        [SerializeField] private string completeGameEndpoint = "/api/player/complete-game-session";

        public async Task<ServerGameResultResponse> SubmitGameResultAsync(string jsonPayload)
        {
            string url = baseUrl.TrimEnd('/') + completeGameEndpoint;

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();

                request.SetRequestHeader("Content-Type", "application/json");

                string token = PlayerApiService.GetToken();

                if (string.IsNullOrWhiteSpace(token))
                {
                    Debug.LogError("Cannot submit game result: missing auth token.");

                    return new ServerGameResultResponse
                    {
                        success = false,
                        message = "Missing auth token. Please login again.",
                        rawJson = "",
                        errorMessage = "Missing auth token"
                    };
                }

                request.SetRequestHeader("Authorization", "Bearer " + token);

                Debug.Log("Submitting game result to: " + url);
                Debug.Log("Authorization header was added.");
                Debug.Log("Payload: " + jsonPayload);

                UnityWebRequestAsyncOperation operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                string responseText = request.downloadHandler.text;

                Debug.Log("Submit game result response code: " + request.responseCode);
                Debug.Log("Submit game result response body: " + responseText);

                if (request.result != UnityWebRequest.Result.Success)
                {
                    return new ServerGameResultResponse
                    {
                        success = false,
                        message = "Failed to submit game result",
                        rawJson = responseText,
                        errorMessage = request.error
                    };
                }

                ServerGameResultResponse response =
                    JsonUtility.FromJson<ServerGameResultResponse>(responseText);

                if (response == null)
                {
                    return new ServerGameResultResponse
                    {
                        success = false,
                        message = "Failed to parse server response",
                        rawJson = responseText,
                        errorMessage = "Invalid JSON response"
                    };
                }

                response.rawJson = responseText;
                return response;
            }
        }
    }
}