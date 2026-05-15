using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

#if ENABLE_INPUT_SYSTEM
using Keyboard = UnityEngine.InputSystem.Keyboard;
#endif

namespace ActiveSaga.Common.GameSession
{
    public class PauseInputController : MonoBehaviour
    {
        [Header("Keyboard Testing")]
        [SerializeField] private bool useEscapeKey = true;

        [Header("Quest 3")]
        [SerializeField] private bool useRightControllerBButton = true;

        [Header("Debug")]
        [SerializeField] private bool logOnlyWhenPauseTriggered = true;

        private bool wasBPressed;
        private UnityEngine.XR.InputDevice cachedRightHand;

        private void Update()
        {
            if (IsEscapePressed())
            {
                TogglePause("Escape");
                return;
            }

            bool bPressed = useRightControllerBButton && IsRightControllerBPressed();

            if (bPressed && !wasBPressed)
            {
                TogglePause("Right Controller B");
            }

            wasBPressed = bPressed;
        }

        private bool IsEscapePressed()
        {
            if (!useEscapeKey)
            {
                return false;
            }

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

        private bool IsRightControllerBPressed()
        {
            UnityEngine.XR.InputDevice rightHand = GetRightHandDevice();

            if (!rightHand.isValid)
            {
                return false;
            }

            bool secondaryButtonPressed;

            bool hasSecondaryButton = rightHand.TryGetFeatureValue(
                CommonUsages.secondaryButton,
                out secondaryButtonPressed
            );

            return hasSecondaryButton && secondaryButtonPressed;
        }

        private UnityEngine.XR.InputDevice GetRightHandDevice()
        {
            if (cachedRightHand.isValid)
            {
                return cachedRightHand;
            }

            cachedRightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

            if (cachedRightHand.isValid)
            {
                return cachedRightHand;
            }

            List<UnityEngine.XR.InputDevice> devices = new List<UnityEngine.XR.InputDevice>();

            InputDevices.GetDevicesWithCharacteristics(
                InputDeviceCharacteristics.Right |
                InputDeviceCharacteristics.Controller |
                InputDeviceCharacteristics.HeldInHand,
                devices
            );

            if (devices.Count > 0)
            {
                cachedRightHand = devices[0];
            }

            return cachedRightHand;
        }

        private void TogglePause(string source)
        {
            if (GameSessionManager.Instance == null)
            {
                Debug.LogWarning("PauseInputController: GameSessionManager.Instance is null.");
                return;
            }

            if (logOnlyWhenPauseTriggered)
            {
                Debug.Log($"PauseInputController: Pause triggered by {source}.");
            }

            GameSessionManager.Instance.TogglePauseGame();
        }
    }
}