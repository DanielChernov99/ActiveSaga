using UnityEngine;
using UnityEngine.XR;

namespace ActiveSaga.Common.GameSession
{
    public class PauseInputController : MonoBehaviour
    {
        [Header("Keyboard Testing")]
        [SerializeField] private bool useEscapeKey = true;

        [Header("XR Controller")]
        [SerializeField] private bool useXRControllerButton = true;
        [SerializeField] private bool checkRightHand = true;
        [SerializeField] private bool checkLeftHand = true;
        [SerializeField] private bool useMenuButton = true;
        [SerializeField] private bool usePrimaryButton = false;
        [SerializeField] private bool useSecondaryButton = false;

        [Header("Headset Presence")]
        [SerializeField] private bool pauseWhenHeadsetRemoved = true;
        [SerializeField] private float headsetCheckInterval = 0.25f;

        private bool wasPausePressed;

        private bool hasLastUserPresence;
        private bool lastUserPresence = true;
        private float nextHeadsetCheckTime;

        private void Update()
        {
            bool isPausePressed = IsPausePressed();

            if (isPausePressed && !wasPausePressed)
            {
                TogglePause();
            }

            wasPausePressed = isPausePressed;

            CheckHeadsetPresence();
        }

        private bool IsPausePressed()
        {
            if (useEscapeKey && Input.GetKeyDown(KeyCode.Escape))
            {
                return true;
            }

            if (!useXRControllerButton)
            {
                return false;
            }

            if (checkRightHand && IsControllerPausePressed(XRNode.RightHand))
            {
                return true;
            }

            if (checkLeftHand && IsControllerPausePressed(XRNode.LeftHand))
            {
                return true;
            }

            return false;
        }

        private bool IsControllerPausePressed(XRNode node)
        {
            if (useMenuButton && ReadBoolFeature(node, CommonUsages.menuButton))
            {
                return true;
            }

            if (usePrimaryButton && ReadBoolFeature(node, CommonUsages.primaryButton))
            {
                return true;
            }

            if (useSecondaryButton && ReadBoolFeature(node, CommonUsages.secondaryButton))
            {
                return true;
            }

            return false;
        }

        private bool ReadBoolFeature(XRNode node, InputFeatureUsage<bool> feature)
        {
            InputDevice device = InputDevices.GetDeviceAtXRNode(node);

            if (!device.isValid)
            {
                return false;
            }

            bool value;
            return device.TryGetFeatureValue(feature, out value) && value;
        }

        private void TogglePause()
        {
            if (GameSessionManager.Instance == null)
            {
                Debug.LogWarning("PauseInputController: GameSessionManager.Instance is null.");
                return;
            }

            GameSessionManager.Instance.TogglePauseGame();
        }

        private void CheckHeadsetPresence()
        {
            if (!pauseWhenHeadsetRemoved)
            {
                return;
            }

            if (Time.unscaledTime < nextHeadsetCheckTime)
            {
                return;
            }

            nextHeadsetCheckTime =
                Time.unscaledTime + Mathf.Max(0.05f, headsetCheckInterval);

            InputDevice hmd = InputDevices.GetDeviceAtXRNode(XRNode.Head);

            if (!hmd.isValid)
            {
                return;
            }

            bool userPresent;

            if (!hmd.TryGetFeatureValue(CommonUsages.userPresence, out userPresent))
            {
                return;
            }

            if (hasLastUserPresence && lastUserPresence && !userPresent)
            {
                PauseOnly();
            }

            lastUserPresence = userPresent;
            hasLastUserPresence = true;
        }

        private void PauseOnly()
        {
            if (GameSessionManager.Instance == null)
            {
                return;
            }

            if (GameSessionManager.Instance.State == GameSessionState.Running)
            {
                GameSessionManager.Instance.PauseGame();
            }
        }
    }
}