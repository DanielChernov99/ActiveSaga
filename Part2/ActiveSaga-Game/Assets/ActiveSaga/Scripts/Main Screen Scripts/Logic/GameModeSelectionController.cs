using ActiveSaga.Common.GameSession;
using ActiveSaga.MainScreen.Data;
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

        private DashboardDataManager dashboardDataManager;

        private SelectedGameMode selectedGameMode = SelectedGameMode.None;
        private SelectedGameDifficulty selectedDifficulty = SelectedGameDifficulty.None;

        private int playerLevel = 1;
        private bool hasLoadedPlayerLevel = false;

        private void Awake()
        {
            if (gameModeSelectionUI == null)
            {
                gameModeSelectionUI = GetComponent<GameModeSelectionUI>();
            }

            dashboardDataManager = DashboardDataManager.Instance;
        }

        private void OnEnable()
        {
            dashboardDataManager = DashboardDataManager.Instance;

            if (dashboardDataManager != null)
            {
                dashboardDataManager.OnDashboardDataLoaded += HandleDashboardDataLoaded;

                if (dashboardDataManager.CurrentData != null)
                {
                    HandleDashboardDataLoaded(dashboardDataManager.CurrentData);
                }
            }
            else
            {
                Debug.LogWarning(
                    "GameModeSelectionController: DashboardDataManager.Instance was not found. " +
                    "Medium and Hard will stay locked until dashboard data is available."
                );
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

            RefreshDifficultyLocks();
            RefreshUI();
        }

        private void OnDisable()
        {
            if (dashboardDataManager != null)
            {
                dashboardDataManager.OnDashboardDataLoaded -= HandleDashboardDataLoaded;
            }
        }

        private void OnDestroy()
        {
            RemoveListeners();
        }

        private void HandleDashboardDataLoaded(DashboardData data)
        {
            if (data == null || data.profile == null)
            {
                Debug.LogWarning("GameModeSelectionController: Dashboard data or profile is missing.");
                return;
            }

            playerLevel = Mathf.Max(1, data.profile.level);
            hasLoadedPlayerLevel = true;

            Debug.Log("GameModeSelectionController: Player level loaded: " + playerLevel);

            RefreshDifficultyLocks();
            RefreshUI();
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
            if (selectedGameMode == SelectedGameMode.Run)
            {
                selectedGameMode = SelectedGameMode.None;
            }
            else
            {
                selectedGameMode = SelectedGameMode.Run;
            }

            RefreshUI();
        }

        public void SelectFightGame()
        {
            if (selectedGameMode == SelectedGameMode.Fight)
            {
                selectedGameMode = SelectedGameMode.None;
            }
            else
            {
                selectedGameMode = SelectedGameMode.Fight;
            }

            RefreshUI();
        }

        public void SelectEasy()
        {
            if (selectedDifficulty == SelectedGameDifficulty.Easy)
            {
                selectedDifficulty = SelectedGameDifficulty.None;
            }
            else
            {
                selectedDifficulty = SelectedGameDifficulty.Easy;
            }

            RefreshUI();
        }

        public void SelectMedium()
        {
            if (selectedDifficulty == SelectedGameDifficulty.Medium)
            {
                selectedDifficulty = SelectedGameDifficulty.None;
                RefreshUI();
                return;
            }

            if (!CanUseDifficulty(SelectedGameDifficulty.Medium))
            {
                Debug.LogWarning("Medium difficulty is locked. Player level: " + playerLevel);
                return;
            }

            selectedDifficulty = SelectedGameDifficulty.Medium;
            RefreshUI();
        }

        public void SelectHard()
        {
            if (selectedDifficulty == SelectedGameDifficulty.Hard)
            {
                selectedDifficulty = SelectedGameDifficulty.None;
                RefreshUI();
                return;
            }

            if (!CanUseDifficulty(SelectedGameDifficulty.Hard))
            {
                Debug.LogWarning("Hard difficulty is locked. Player level: " + playerLevel);
                return;
            }

            selectedDifficulty = SelectedGameDifficulty.Hard;
            RefreshUI();
        }

        private bool CanUseDifficulty(SelectedGameDifficulty difficulty)
        {
            if (difficulty == SelectedGameDifficulty.Easy)
            {
                return true;
            }

            if (!hasLoadedPlayerLevel)
            {
                return false;
            }

            if (difficulty == SelectedGameDifficulty.Medium)
            {
                return playerLevel >= 11;
            }

            if (difficulty == SelectedGameDifficulty.Hard)
            {
                return playerLevel >= 21;
            }

            return false;
        }

        private void RefreshDifficultyLocks()
        {
            if (easyButton != null)
            {
                easyButton.interactable = true;
            }

            if (mediumButton != null)
            {
                mediumButton.interactable = CanUseDifficulty(SelectedGameDifficulty.Medium);
            }

            if (hardButton != null)
            {
                hardButton.interactable = CanUseDifficulty(SelectedGameDifficulty.Hard);
            }

            if (selectedDifficulty != SelectedGameDifficulty.None &&
                !CanUseDifficulty(selectedDifficulty))
            {
                selectedDifficulty = SelectedGameDifficulty.None;
            }
        }

        private void RefreshUI()
        {
            RefreshDifficultyLocks();

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

            if (!CanUseDifficulty(selectedDifficulty))
            {
                Debug.LogWarning(
                    "Selected difficulty is locked. Player level: " + playerLevel +
                    ", Difficulty: " + selectedDifficulty
                );

                selectedDifficulty = SelectedGameDifficulty.Easy;
                RefreshUI();
                return;
            }

            GameLaunchData.SetSelection(selectedGameMode, selectedDifficulty);

            Debug.Log(
                "Launching game. Mode: " + selectedGameMode +
                ", Difficulty: " + selectedDifficulty +
                ", Player level: " + playerLevel
            );

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