using ActiveSaga.Common.GameSession;
using ActiveSaga.MainScreen.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ActiveSaga.MainScreen.Logic
{
    public class GameModeSelectionController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameModeSelectionUI gameModeSelectionUI;

        [Header("Game Mode Buttons")]
        [SerializeField] private Button runGameButton;
        [SerializeField] private Button fightGameButton;

        [Header("Difficulty Buttons")]
        [SerializeField] private Button easyButton;
        [SerializeField] private Button mediumButton;
        [SerializeField] private Button hardButton;

        [Header("Play Button")]
        [SerializeField] private Button playButton;

        [Header("Scene Names")]
        [SerializeField] private string runGameSceneName = "RunGame";
        [SerializeField] private string fightGameSceneName = "FightGame";

        [Header("Default Selection")]
        [SerializeField] private bool selectDefaultOnStart = true;
        [SerializeField] private SelectedGameMode defaultGameMode = SelectedGameMode.Run;
        [SerializeField] private SelectedGameDifficulty defaultDifficulty = SelectedGameDifficulty.Easy;

        private SelectedGameMode selectedGameMode = SelectedGameMode.None;
        private SelectedGameDifficulty selectedDifficulty = SelectedGameDifficulty.None;

        private void Awake()
        {
            if (gameModeSelectionUI == null)
            {
                gameModeSelectionUI = GetComponent<GameModeSelectionUI>();
            }
        }

        private void Start()
        {
            AddListeners();

            if (selectDefaultOnStart)
            {
                selectedGameMode = defaultGameMode;
                selectedDifficulty = defaultDifficulty;
            }

            RefreshUI();
        }

        private void OnDestroy()
        {
            RemoveListeners();
        }

        private void AddListeners()
        {
            if (runGameButton != null)
            {
                runGameButton.onClick.AddListener(SelectRunGame);
            }

            if (fightGameButton != null)
            {
                fightGameButton.onClick.AddListener(SelectFightGame);
            }

            if (easyButton != null)
            {
                easyButton.onClick.AddListener(SelectEasy);
            }

            if (mediumButton != null)
            {
                mediumButton.onClick.AddListener(SelectMedium);
            }

            if (hardButton != null)
            {
                hardButton.onClick.AddListener(SelectHard);
            }

            if (playButton != null)
            {
                playButton.onClick.AddListener(Play);
            }
        }

        private void RemoveListeners()
        {
            if (runGameButton != null)
            {
                runGameButton.onClick.RemoveListener(SelectRunGame);
            }

            if (fightGameButton != null)
            {
                fightGameButton.onClick.RemoveListener(SelectFightGame);
            }

            if (easyButton != null)
            {
                easyButton.onClick.RemoveListener(SelectEasy);
            }

            if (mediumButton != null)
            {
                mediumButton.onClick.RemoveListener(SelectMedium);
            }

            if (hardButton != null)
            {
                hardButton.onClick.RemoveListener(SelectHard);
            }

            if (playButton != null)
            {
                playButton.onClick.RemoveListener(Play);
            }
        }

        public void SelectRunGame()
        {
            selectedGameMode = SelectedGameMode.Run;
            RefreshUI();
        }

        public void SelectFightGame()
        {
            selectedGameMode = SelectedGameMode.Fight;
            RefreshUI();
        }

        public void SelectEasy()
        {
            selectedDifficulty = SelectedGameDifficulty.Easy;
            RefreshUI();
        }

        public void SelectMedium()
        {
            selectedDifficulty = SelectedGameDifficulty.Medium;
            RefreshUI();
        }

        public void SelectHard()
        {
            selectedDifficulty = SelectedGameDifficulty.Hard;
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (gameModeSelectionUI != null)
            {
                gameModeSelectionUI.Render(selectedGameMode, selectedDifficulty);
            }
        }

        private void Play()
        {
            if (selectedGameMode == SelectedGameMode.None)
            {
                Debug.LogWarning("No game mode selected.");
                return;
            }

            if (selectedDifficulty == SelectedGameDifficulty.None)
            {
                Debug.LogWarning("No difficulty selected.");
                return;
            }

            GameLaunchData.SetSelection(selectedGameMode, selectedDifficulty);

            if (selectedGameMode == SelectedGameMode.Run)
            {
                SceneManager.LoadScene(runGameSceneName);
                return;
            }

            if (selectedGameMode == SelectedGameMode.Fight)
            {
                SceneManager.LoadScene(fightGameSceneName);
                return;
            }
        }
    }
}