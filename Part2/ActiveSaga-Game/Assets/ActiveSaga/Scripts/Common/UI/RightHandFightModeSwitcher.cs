using UnityEngine;
using ActiveSaga.Common.GameSession;

namespace ActiveSaga.Common.UI
{
    public class RightHandFightModeSwitcher : MonoBehaviour
    {
        [Header("Sword")]
        [SerializeField] private GameObject swordRoot;

        [Header("All Right Hand Interaction Objects To Hide During Combat")]
        [SerializeField] private GameObject[] rightHandInteractionObjects;

        [Header("Only These Will Be Enabled In Menu")]
        [SerializeField] private GameObject[] menuRayObjects;

        [Header("Behavior")]
        [SerializeField] private bool showRayWhenPaused = true;
        [SerializeField] private bool showRayWhenGameEnded = true;
        [SerializeField] private bool showRayWhenWaitingForServer = false;

        [Header("Debug")]
        [SerializeField] private bool logStateChanges = true;

        private GameSessionState? lastState;
        private bool? lastMenuMode;

        private void Awake()
        {
            ApplyMode(force: true);
        }

        private void Start()
        {
            ApplyMode(force: true);
        }

        private void OnEnable()
        {
            ApplyMode(force: true);
        }

        private void LateUpdate()
        {
            ApplyMode(force: true);
        }

        private void ApplyMode(bool force)
        {
            bool menuMode = ShouldUseMenuRay();
            GameSessionState? currentState = GetCurrentState();

            if (logStateChanges && (lastState != currentState || lastMenuMode != menuMode))
            {
                Debug.Log($"RightHandFightModeSwitcher: State = {currentState}, MenuMode = {menuMode}");
                lastState = currentState;
                lastMenuMode = menuMode;
            }

            SetActiveSafe(swordRoot, !menuMode);

            // First, turn off every right-hand interaction/ray object.
            if (rightHandInteractionObjects != null)
            {
                foreach (GameObject obj in rightHandInteractionObjects)
                {
                    SetActiveSafe(obj, false);
                }
            }

            // Then, only in Pause/End menu, turn on the specific UI ray object.
            if (menuRayObjects != null)
            {
                foreach (GameObject obj in menuRayObjects)
                {
                    SetActiveSafe(obj, menuMode);
                }
            }
        }

        private bool ShouldUseMenuRay()
        {
            if (GameSessionManager.Instance == null)
            {
                return false;
            }

            GameSessionState state = GameSessionManager.Instance.State;

            if (showRayWhenPaused && state == GameSessionState.Paused)
            {
                return true;
            }

            if (showRayWhenGameEnded && state == GameSessionState.Ended)
            {
                return true;
            }

            if (showRayWhenWaitingForServer && state == GameSessionState.WaitingForServer)
            {
                return true;
            }

            return false;
        }

        private GameSessionState? GetCurrentState()
        {
            if (GameSessionManager.Instance == null)
            {
                return null;
            }

            return GameSessionManager.Instance.State;
        }

        private void SetActiveSafe(GameObject obj, bool active)
        {
            if (obj == null)
            {
                return;
            }

            if (obj.activeSelf != active)
            {
                obj.SetActive(active);
            }
        }
    }
}