using UnityEngine;
using UnityEngine.XR;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ActiveSaga.Common.GameSession
{
    public class PauseInputController : MonoBehaviour
    {
        [Header("Keyboard Testing")]
        [SerializeField] private bool useEscapeKey = true;

        [Header("Quest 3 Controller")]
        [SerializeField] private bool useXRControllerButton = true;

        [Tooltip("Quest 3 A button is on the right controller.")]
        [SerializeField] private bool checkRightHand = true;

        [SerializeField] private bool checkLeftHand = false;

        [Header("Quest 3 Button Mapping")]
        [Tooltip("Quest 3 right controller A button = Primary Button.")]
        [SerializeField] private bool usePrimaryButton = true;

        [Tooltip("Quest 3 right controller B button = Secondary Button.")]
        [SerializeField] private bool useSecondaryButton = false;

        [SerializeField] private bool useMenuButton = false;

        [Header("Headset Presence")]
        [SerializeField] private bool pauseWhenHeadsetRemoved = false;
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
            if (useEscapeKey && IsKeyboardPausePressed())
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

        private bool IsKeyboardPausePressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                return true;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                return true;
            }
#endif

            return false;
        }

        private bool IsControllerPausePressed(XRNode node)
        {
            if (usePrimaryButton && ReadBoolFeature(node, UnityEngine.XR.CommonUsages.primaryButton))
            {
                return true;
            }

            if (useSecondaryButton && ReadBoolFeature(node, UnityEngine.XR.CommonUsages.secondaryButton))
            {
                return true;
            }

            if (useMenuButton && ReadBoolFeature(node, UnityEngine.XR.CommonUsages.menuButton))
            {
                return true;
            }

            return false;
        }

        private bool ReadBoolFeature(XRNode node, InputFeatureUsage<bool> feature)
        {
            UnityEngine.XR.InputDevice device = InputDevices.GetDeviceAtXRNode(node);

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

            Debug.Log("PauseInputController: Pause button detected.");

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

            UnityEngine.XR.InputDevice hmd = InputDevices.GetDeviceAtXRNode(XRNode.Head);

            if (!hmd.isValid)
            {
                return;
            }

            bool userPresent;

            if (!hmd.TryGetFeatureValue(UnityEngine.XR.CommonUsages.userPresence, out userPresent))
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
                Debug.Log("PauseInputController: Headset removed, pausing game.");
                GameSessionManager.Instance.PauseGame();
            }
        }
    }
}