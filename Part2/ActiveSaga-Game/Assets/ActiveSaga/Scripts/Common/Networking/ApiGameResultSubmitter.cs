using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ActiveSaga.Common.Networking
{
    public class ApiGameResultSubmitter : MonoBehaviour, IGameResultSubmitter
    {
        [Header("API Settings")]
        [SerializeField] private string baseUrl = "http://localhost:3000";
        [SerializeField] private string completeGameEndpoint = "/api/game-sessions/complete";

        public async Task<ServerGameResultResponse> SubmitGameResultAsync(string jsonPayload)
        {
            string url = baseUrl.TrimEnd('/') + completeGameEndpoint;

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);

                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();

                request.SetRequestHeader("Content-Type", "application/json");

                string token = AuthTokenProvider.Instance != null
                    ? AuthTokenProvider.Instance.Token
                    : "";

                if (!string.IsNullOrWhiteSpace(token))
                {
                    request.SetRequestHeader("Authorization", "Bearer " + token);
                }

                UnityWebRequestAsyncOperation operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                string responseText = request.downloadHandler.text;

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