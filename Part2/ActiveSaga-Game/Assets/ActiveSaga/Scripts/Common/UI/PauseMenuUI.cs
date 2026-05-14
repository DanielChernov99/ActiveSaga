using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ActiveSaga.Common.GameSession;

namespace ActiveSaga.Common.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        public enum QuitAction
        {
            SubmitResultAndShowResults,
            LoadMainMenuWithoutSubmitting
        }

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button quitButton;

        [Header("Quit Settings")]
        [SerializeField] private QuitAction quitAction = QuitAction.SubmitResultAndShowResults;
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Header("Optional Events")]
        [Tooltip("Use this to stop Fight Game systems before quitting. For example, connect WaveManager.StopWavesAfterGameEnded here.")]
        [SerializeField] private UnityEvent beforeQuit;

        private void OnEnable()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(ResumeGame);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(QuitGame);
            }
        }

        private void OnDisable()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveListener(ResumeGame);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(QuitGame);
            }
        }

        public void ResumeGame()
        {
            if (GameSessionManager.Instance != null)
            {
                GameSessionManager.Instance.ResumeGame();
            }
        }

        public void QuitGame()
        {
            beforeQuit?.Invoke();

            if (quitAction == QuitAction.SubmitResultAndShowResults)
            {
                if (GameSessionManager.Instance != null)
                {
                    GameSessionManager.Instance.EndGameAsPlayerQuit();
                    return;
                }

                Debug.LogWarning("PauseMenuUI: GameSessionManager is missing. Loading main menu instead.");
            }

            LoadMainMenu();
        }

        private void LoadMainMenu()
        {
            Time.timeScale = 1f;

            if (string.IsNullOrWhiteSpace(mainMenuSceneName))
            {
                Debug.LogError("PauseMenuUI: Main menu scene name is empty.");
                return;
            }

            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}