using ActiveSaga.Common.GameSession;
using UnityEngine;
using UnityEngine.UI;

namespace ActiveSaga.MainScreen.UI
{
    public class GameModeSelectionUI : MonoBehaviour
    {
        [Header("Selected Game Frames")]
        [SerializeField] private GameObject chosenRunGameFrame;
        [SerializeField] private GameObject chosenFightGameFrame;

        [Header("Selected Difficulty Frames")]
        [SerializeField] private GameObject chosenEasyFrame;
        [SerializeField] private GameObject chosenMediumFrame;
        [SerializeField] private GameObject chosenHardFrame;

        [Header("Play Button")]
        [SerializeField] private Button playButton;

        public void Render(SelectedGameMode selectedGameMode, SelectedGameDifficulty selectedDifficulty)
        {
            SetActive(chosenRunGameFrame, selectedGameMode == SelectedGameMode.Run);
            SetActive(chosenFightGameFrame, selectedGameMode == SelectedGameMode.Fight);

            SetActive(chosenEasyFrame, selectedDifficulty == SelectedGameDifficulty.Easy);
            SetActive(chosenMediumFrame, selectedDifficulty == SelectedGameDifficulty.Medium);
            SetActive(chosenHardFrame, selectedDifficulty == SelectedGameDifficulty.Hard);

            bool canPlay =
                selectedGameMode != SelectedGameMode.None &&
                selectedDifficulty != SelectedGameDifficulty.None;

            if (playButton != null)
            {
                playButton.interactable = canPlay;
            }
        }

        private void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}