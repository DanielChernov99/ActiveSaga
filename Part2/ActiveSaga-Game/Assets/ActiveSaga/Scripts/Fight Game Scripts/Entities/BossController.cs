using System.Collections;
using UnityEngine;
using ActiveSaga.Common.GameSession;
using ActiveSaga.BossFight.Core;

namespace ActiveSaga.BossFight.Entities
{
    public class BossController : MonoBehaviour
    {
        public static BossController Instance { get; private set; }

        [SerializeField] private Animator animator;
        [SerializeField] private float maxHP = 1000f;
        [SerializeField] private float gameWonDelaySeconds = 2f;

        private float currentHP;
        private bool gameWonTriggered;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Debug.Log($"[BossController] Instance Initialized: {gameObject.name} (ID: {GetInstanceID()})");
            }
            else
            {
                Debug.LogWarning($"[BossController] Duplicate instance found on {gameObject.name}. Destroying it.");
                Destroy(gameObject);
                return;
            }

            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            currentHP = maxHP;
            gameWonTriggered = false;
        }

        private void Start()
        {
            EventManager.Trigger(new HealthChangedEvent
            {
                current = currentHP,
                max = maxHP,
                isPlayer = false
            });
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                Debug.Log($"[BossController] Instance Cleared: {gameObject.name}");
            }
        }

        public void TakeDamage(float amount)
        {
            if (gameWonTriggered)
            {
                return;
            }

            if (currentHP <= 0f)
            {
                return;
            }

            float oldHP = currentHP;

            currentHP -= amount;

            if (currentHP < 0f)
            {
                currentHP = 0f;
            }

            Debug.Log($"[BossController] Damage Received: {amount}. HP: {oldHP} -> {currentHP} / {maxHP}");

            EventManager.Trigger(new HealthChangedEvent
            {
                current = currentHP,
                max = maxHP,
                isPlayer = false
            });

            if (currentHP <= 0f)
            {
                OnBossDefeated();
            }
        }

        private void OnBossDefeated()
        {
            if (gameWonTriggered)
            {
                return;
            }

            gameWonTriggered = true;

            Debug.Log("<color=red>[BossController] Boss Defeated!</color>");

            PlayAnimation("Die");

            StartCoroutine(EndGameAsWonAfterDelay());
        }

        private IEnumerator EndGameAsWonAfterDelay()
        {
            yield return new WaitForSeconds(gameWonDelaySeconds);

            if (GameSessionManager.Instance != null)
            {
                GameSessionManager.Instance.EndGameAsGameWon();
            }
            else
            {
                Debug.LogError("[BossController] Cannot end game as won because GameSessionManager.Instance is null.");
            }
        }

        public void PlayAnimation(string trigger)
        {
            if (animator != null && !string.IsNullOrEmpty(trigger))
            {
                animator.SetTrigger(trigger);
            }
        }
    }
}