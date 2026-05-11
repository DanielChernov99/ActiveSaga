using System;
using UnityEngine;
using ActiveSaga.Common.Networking;
using ActiveSaga.Common.UI;

namespace ActiveSaga.Common.GameSession
{
    public class GameSessionManager : MonoBehaviour
    {
        public static GameSessionManager Instance { get; private set; }

        [Header("Session Setup")]
        [SerializeField] private GameType gameType;
        [SerializeField] private bool startSessionOnStart = true;

        [Header("Required References")]
        [SerializeField] private GameStatsTracker statsTracker;

        [Tooltip("Drag here a component that implements IGameResultSubmitter, for example ApiGameResultSubmitter or MockGameResultSubmitter.")]
        [SerializeField] private MonoBehaviour submitterBehaviour;

        [Header("UI")]
        [SerializeField] private GameObject gameplayHUD;
        [SerializeField] private EndGameResultsViewBase resultsUI;
        private IGameResultSubmitter gameResultSubmitter;

        private string sessionId;
        private string startedUtc;

        private float startedRealtime;
        private float pausedStartedRealtime;
        private float totalPausedSeconds;

        private GameSessionState state = GameSessionState.NotStarted;

        public GameSessionState State => state;
        public GameType GameType => gameType;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            ResolveSubmitter();
        }

        private void Start()
        {
            if (startSessionOnStart)
            {
                StartSession();
            }
        }

        private void ResolveSubmitter()
        {
            gameResultSubmitter = null;

            if (submitterBehaviour != null)
            {
                if (submitterBehaviour is IGameResultSubmitter submitter)
                {
                    gameResultSubmitter = submitter;
                    Debug.Log("GameSessionManager using connected submitter: " + submitterBehaviour.GetType().Name);
                    return;
                }

                Debug.LogWarning(
                    "Submitter Behaviour is connected, but it does not implement IGameResultSubmitter. Connected component: "
                    + submitterBehaviour.GetType().Name +
                    ". Trying to find a valid submitter automatically."
                );
            }

            MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();

            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IGameResultSubmitter foundSubmitter)
                {
                    submitterBehaviour = behaviour;
                    gameResultSubmitter = foundSubmitter;

                    Debug.Log("GameSessionManager found submitter automatically: " + behaviour.GetType().Name);
                    return;
                }
            }

            Debug.LogError(
                "No component implementing IGameResultSubmitter found. " +
                "Add MockGameResultSubmitter or ApiGameResultSubmitter to the same object as GameSessionManager."
            );
        }

        public void StartSession()
        {
            if (state == GameSessionState.Running)
            {
                return;
            }

            Time.timeScale = 1f;

            sessionId = Guid.NewGuid().ToString();
            startedUtc = DateTime.UtcNow.ToString("o");

            startedRealtime = Time.realtimeSinceStartup;
            pausedStartedRealtime = 0f;
            totalPausedSeconds = 0f;

            if (statsTracker != null)
            {
                statsTracker.ResetStats();
            }
            else
            {
                Debug.LogWarning("GameSessionManager: Stats Tracker is missing.");
            }

            ResolveSubmitter();

            if (gameplayHUD != null)
            {
                gameplayHUD.SetActive(true);
            }

            if (resultsUI != null)
            {
                resultsUI.HideAll();
            }

            state = GameSessionState.Running;

            Debug.Log("Game Session Started: " + gameType + ", Session ID: " + sessionId);
        }

        public void PauseGame()
        {
            if (state != GameSessionState.Running)
            {
                return;
            }

            pausedStartedRealtime = Time.realtimeSinceStartup;
            state = GameSessionState.Paused;

            Time.timeScale = 0f;

            Debug.Log("Game Paused");
        }

        public void ResumeGame()
        {
            if (state != GameSessionState.Paused)
            {
                return;
            }

            totalPausedSeconds += Time.realtimeSinceStartup - pausedStartedRealtime;
            pausedStartedRealtime = 0f;

            Time.timeScale = 1f;
            state = GameSessionState.Running;

            Debug.Log("Game Resumed");
        }

        public async void EndGame(GameEndReason endReason)
        {
            if (state == GameSessionState.Ended || state == GameSessionState.WaitingForServer)
            {
                return;
            }

            if (state == GameSessionState.NotStarted)
            {
                Debug.LogWarning("GameSessionManager: EndGame was called before StartSession.");
            }

            if (state == GameSessionState.Paused)
            {
                totalPausedSeconds += Time.realtimeSinceStartup - pausedStartedRealtime;
                pausedStartedRealtime = 0f;
            }

            Time.timeScale = 1f;

            float durationSeconds = CalculateActiveDurationSeconds();

            GameStatsSnapshot statsSnapshot = null;

            if (statsTracker != null)
            {
                statsSnapshot = statsTracker.BuildSnapshot();
            }
            else
            {
                Debug.LogWarning("GameSessionManager: Cannot build stats snapshot because Stats Tracker is missing.");
            }

            string endedUtc = DateTime.UtcNow.ToString("o");

            string payloadJson = GameSessionPayloadBuilder.BuildJson(
                sessionId,
                startedUtc,
                endedUtc,
                gameType,
                endReason,
                durationSeconds,
                statsSnapshot
            );

            Debug.Log("Payload sent to server:");
            Debug.Log(payloadJson);

            state = GameSessionState.WaitingForServer;

            if (gameplayHUD != null)
            {
                gameplayHUD.SetActive(false);
            }

            if (resultsUI != null)
            {
                resultsUI.ShowLoading();
            }

            if (gameResultSubmitter == null)
            {
                ResolveSubmitter();
            }

            if (gameResultSubmitter == null)
            {
                state = GameSessionState.Ended;

                if (resultsUI != null)
                {
                    resultsUI.ShowError("No game result submitter connected.");
                }

                Debug.LogError("No game result submitter connected.");
                return;
            }

            ServerGameResultResponse response =
                await gameResultSubmitter.SubmitGameResultAsync(payloadJson);

            state = GameSessionState.Ended;

            if (response == null)
            {
                if (resultsUI != null)
                {
                    resultsUI.ShowError("No response from server.");
                }

                Debug.LogError("No response from server.");
                return;
            }

            if (!response.success)
            {
                string errorMessage = !string.IsNullOrWhiteSpace(response.errorMessage)
                    ? response.errorMessage
                    : response.message;

                if (string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = "Failed to save game result.";
                }

                if (resultsUI != null)
                {
                    resultsUI.ShowError(errorMessage);
                }

                Debug.LogError(errorMessage);

                if (!string.IsNullOrWhiteSpace(response.rawJson))
                {
                    Debug.LogError(response.rawJson);
                }

                return;
            }

            if (resultsUI != null)
            {
                resultsUI.ShowResults(response, endReason, durationSeconds, statsSnapshot);
            }

            Debug.Log("Server response:");

            if (!string.IsNullOrWhiteSpace(response.rawJson))
            {
                Debug.Log(response.rawJson);
            }
            else
            {
                Debug.Log(response.message);
            }
        }

        public void EndGameAsGameOver()
        {
            EndGame(GameEndReason.GameOver);
        }

        public void EndGameAsGameWon()
        {
            EndGame(GameEndReason.GameWon);
        }

        public void EndGameAsPlayerQuit()
        {
            EndGame(GameEndReason.PlayerQuit);
        }

        public float GetActiveDurationSeconds()
        {
            if (state == GameSessionState.NotStarted)
            {
                return 0f;
            }

            if (state == GameSessionState.Paused)
            {
                float pausedDuration = pausedStartedRealtime - startedRealtime - totalPausedSeconds;
                return Mathf.Max(0f, pausedDuration);
            }

            return CalculateActiveDurationSeconds();
        }

        private float CalculateActiveDurationSeconds()
        {
            float now = Time.realtimeSinceStartup;
            float activeDuration = now - startedRealtime - totalPausedSeconds;
            return Mathf.Max(0f, activeDuration);
        }
    }
}