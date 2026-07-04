using UnityEngine;
using ActiveSaga.Common.GameSession;

namespace ActiveSaga.BossFight.Core
{
    public class FightKeepMovingController : MonoBehaviour
    {
        [Header("Movement Sources")]
        [SerializeField] private Transform headTransform;
        [SerializeField] private Transform rightHandTransform;

        [Header("Movement Settings")]
        [SerializeField] private float minimumMovementSpeed = 0.12f;
        [SerializeField] private float idleWarningTime = 3.5f;
        [SerializeField] private float warningCooldown = 3f;

        [Header("Light Bonus Feedback")]
        [SerializeField] private float continuousMoveBonusTime = 8f;
        [SerializeField] private string keepMovingMessage = "Keep moving!";
        [SerializeField] private string bonusMessage = "Energy Bonus +5";
        [SerializeField] private float feedbackDuration = 1.2f;

        private Vector3 lastHeadPosition;
        private Vector3 lastRightHandPosition;

        private float idleTimer;
        private float movementTimer;
        private float lastWarningTime = -999f;
        private bool initialized;

        private void Start()
        {
            ResolveReferences();
            InitializePositions();
        }

        private void Update()
        {
            if (!CanRun())
            {
                return;
            }

            ResolveReferences();

            if (!initialized)
            {
                InitializePositions();
                return;
            }

            float speed = CalculateMovementSpeed();
            bool isMoving = speed >= minimumMovementSpeed;

            if (isMoving)
            {
                idleTimer = 0f;
                movementTimer += Time.deltaTime;

                if (movementTimer >= continuousMoveBonusTime)
                {
                    movementTimer = 0f;

                    EventManager.Trigger(new FeedbackEvent
                    {
                        message = bonusMessage,
                        duration = feedbackDuration
                    });
                }
            }
            else
            {
                idleTimer += Time.deltaTime;
                movementTimer = 0f;

                if (idleTimer >= idleWarningTime && Time.time - lastWarningTime >= warningCooldown)
                {
                    lastWarningTime = Time.time;

                    EventManager.Trigger(new FeedbackEvent
                    {
                        message = keepMovingMessage,
                        duration = feedbackDuration
                    });
                }
            }
        }

        private void ResolveReferences()
        {
            if (headTransform == null)
            {
                if (BossFightGameManager.Instance != null && BossFightGameManager.Instance.PlayerCamera != null)
                {
                    headTransform = BossFightGameManager.Instance.PlayerCamera.transform;
                }
                else if (Camera.main != null)
                {
                    headTransform = Camera.main.transform;
                }
            }
        }

        private void InitializePositions()
        {
            if (headTransform == null)
            {
                return;
            }

            lastHeadPosition = headTransform.position;

            if (rightHandTransform != null)
            {
                lastRightHandPosition = rightHandTransform.position;
            }

            initialized = true;
        }

        private float CalculateMovementSpeed()
        {
            float maxSpeed = 0f;

            if (headTransform != null)
            {
                float headSpeed = (headTransform.position - lastHeadPosition).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
                maxSpeed = Mathf.Max(maxSpeed, headSpeed);
                lastHeadPosition = headTransform.position;
            }

            if (rightHandTransform != null)
            {
                float handSpeed = (rightHandTransform.position - lastRightHandPosition).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
                maxSpeed = Mathf.Max(maxSpeed, handSpeed);
                lastRightHandPosition = rightHandTransform.position;
            }

            return maxSpeed;
        }

        private bool CanRun()
        {
            if (GameSessionManager.Instance == null)
            {
                return true;
            }

            GameSessionState state = GameSessionManager.Instance.State;

            return state != GameSessionState.Paused &&
                   state != GameSessionState.WaitingForServer &&
                   state != GameSessionState.Ended;
        }
    }
}
