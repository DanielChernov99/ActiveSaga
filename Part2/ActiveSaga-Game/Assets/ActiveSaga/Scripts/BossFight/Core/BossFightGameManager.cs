using UnityEngine;
using ActiveSaga.BossFight.Waves;

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
        [SerializeField] private Transform playerTransform; // Root of XR Rig
        [SerializeField] private Camera playerCamera;

        private float currentPlayerHP;

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
        }

        private void ValidateDependencies()
        {
            bool hasError = false;
            if (waveManager == null) { Debug.LogError("WaveManager reference missing in BossFightGameManager!"); hasError = true; }
            if (poolManager == null) { Debug.LogError("PoolManager reference missing in BossFightGameManager!"); hasError = true; }
            if (uiManager == null) { Debug.LogError("UIManager reference missing in BossFightGameManager!"); hasError = true; }
            if (playerTransform == null) { Debug.LogError("PlayerTransform (XR Origin) missing in BossFightGameManager!"); hasError = true; }
            
            if (!hasError)
            {
                Debug.Log("<color=green>BossFight Bootstrap: All dependencies validated.</color>");
            }
        }

        private void Start()
        {
            EventManager.Trigger(new HealthChangedEvent { current = currentPlayerHP, max = maxPlayerHP, isPlayer = true });
        }

        public void TakeDamage(float amount)
        {
            currentPlayerHP -= amount;
            if (currentPlayerHP < 0) currentPlayerHP = 0;
            
            EventManager.Trigger(new HealthChangedEvent { current = currentPlayerHP, max = maxPlayerHP, isPlayer = true });

            if (currentPlayerHP <= 0)
            {
                OnGameOver();
            }
        }

        private void OnGameOver()
        {
            EventManager.Trigger(new FeedbackEvent { message = "Game Over", duration = 10f });
        }
    }
}

