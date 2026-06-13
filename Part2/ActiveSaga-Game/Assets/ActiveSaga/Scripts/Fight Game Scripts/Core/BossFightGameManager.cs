using UnityEngine;
using ActiveSaga.BossFight.Waves;
using ActiveSaga.BossFight.Entities;
using ActiveSaga.Common.GameSession;

namespace ActiveSaga.BossFight.Core
{
    public class BossFightGameManager : MonoBehaviour
    {
        public static BossFightGameManager Instance { get; private set; }

        [Header("Managers")]
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private PoolManager poolManager;
        [SerializeField] private UIManager uiManager;

        [Header("Player Settings")]
        [SerializeField] private float maxPlayerHP = 100f;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Camera playerCamera;

        [Header("Boss End Game References")]
        [SerializeField] private Transform bossAudioRoot;

        [Header("End Game Behavior")]
        [SerializeField] private bool stopMusicWhenGameEnds = true;
        [SerializeField] private bool stopBossAudioWhenGameEnds = true;
        [SerializeField] private bool stopBossAnimationsWhenGameEnds = true;

        private float currentPlayerHP;
        private bool gameOverTriggered;

        public WaveManager WaveManager => waveManager;
        public PoolManager PoolManager => poolManager;
        public Transform PlayerTransform => playerTransform;
        public Camera PlayerCamera => playerCamera;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError($"Multiple instances of BossFightGameManager found on {gameObject.name}. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            ValidateDependencies();

            currentPlayerHP = maxPlayerHP;
            gameOverTriggered = false;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void ValidateDependencies()
        {
            bool hasError = false;

            if (waveManager == null)
            {
                Debug.LogError("WaveManager reference missing in BossFightGameManager!");
                hasError = true;
            }

            if (poolManager == null)
            {
                Debug.LogError("PoolManager reference missing in BossFightGameManager!");
                hasError = true;
            }

            if (uiManager == null)
            {
                Debug.LogError("UIManager reference missing in BossFightGameManager!");
                hasError = true;
            }

            if (playerTransform == null)
            {
                Debug.LogError("PlayerTransform (XR Origin) missing in BossFightGameManager!");
                hasError = true;
            }

            if (!hasError)
            {
                Debug.Log("<color=green>BossFight Bootstrap: All dependencies validated.</color>");
            }
        }

        private void Start()
        {
            EventManager.Trigger(new HealthChangedEvent
            {
                current = currentPlayerHP,
                max = maxPlayerHP,
                isPlayer = true
            });
        }

        public void TakeDamage(float amount)
        {
            if (gameOverTriggered)
            {
                return;
            }

            if (currentPlayerHP <= 0f)
            {
                return;
            }

            currentPlayerHP -= amount;

            if (currentPlayerHP < 0f)
            {
                currentPlayerHP = 0f;
            }

            Debug.Log($"[BossFightGameManager] Player took damage: {amount}. HP: {currentPlayerHP}/{maxPlayerHP}");

            EventManager.Trigger(new HealthChangedEvent
            {
                current = currentPlayerHP,
                max = maxPlayerHP,
                isPlayer = true
            });

            if (currentPlayerHP <= 0f)
            {
                OnGameOver();
            }
        }

        public void StopFightGameBeforeQuit()
        {
            if (gameOverTriggered)
            {
                return;
            }

            gameOverTriggered = true;

            StopBossAndMusic();
        }

        private void OnGameOver()
        {
            if (gameOverTriggered)
            {
                return;
            }

            gameOverTriggered = true;

            Debug.Log("<color=red>[BossFightGameManager] GAME OVER TRIGGERED</color>");

            EventManager.Trigger(new FeedbackEvent
            {
                message = "Game Over",
                duration = 10f
            });

            StopFightGameplaySystems();

            if (GameSessionManager.Instance != null)
            {
                GameSessionManager.Instance.EndGameAsGameOver();
            }
            else
            {
                Debug.LogError("[BossFightGameManager] Cannot end game as GameOver because GameSessionManager.Instance is null.");
            }
        }

        private void StopFightGameplaySystems()
        {
            if (waveManager != null)
            {
                waveManager.StopWavesAfterGameEnded();
            }

            StopBossAndMusic();
        }

        private void StopBossAndMusic()
        {
            if (stopMusicWhenGameEnds && ActiveSagaAudioManager.Instance != null)
            {
                ActiveSagaAudioManager.Instance.StopMusic();
            }

            StopBossAfterGameEnded();
        }

        private void StopBossAfterGameEnded()
        {
            Transform targetRoot = bossAudioRoot;

            if (targetRoot == null && BossController.Instance != null)
            {
                targetRoot = BossController.Instance.transform.parent;
            }

            if (targetRoot == null && BossController.Instance != null)
            {
                targetRoot = BossController.Instance.transform;
            }

            if (targetRoot == null)
            {
                return;
            }

            MonoBehaviour[] behaviours = targetRoot.GetComponentsInChildren<MonoBehaviour>(true);

            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] == null)
                {
                    continue;
                }

                string typeName = behaviours[i].GetType().Name;

                if (typeName == "Random3DAudioEmitter" ||
                    typeName == "Looping3DAudioEmitter")
                {
                    behaviours[i].enabled = false;
                }
            }

            if (stopBossAudioWhenGameEnds)
            {
                AudioSource[] audioSources = targetRoot.GetComponentsInChildren<AudioSource>(true);

                for (int i = 0; i < audioSources.Length; i++)
                {
                    if (audioSources[i] != null)
                    {
                        audioSources[i].Stop();
                        audioSources[i].enabled = false;
                    }
                }
            }

            if (stopBossAnimationsWhenGameEnds)
            {
                Animator[] animators = targetRoot.GetComponentsInChildren<Animator>(true);

                for (int i = 0; i < animators.Length; i++)
                {
                    if (animators[i] != null)
                    {
                        animators[i].enabled = false;
                    }
                }
            }
        }
    }
}