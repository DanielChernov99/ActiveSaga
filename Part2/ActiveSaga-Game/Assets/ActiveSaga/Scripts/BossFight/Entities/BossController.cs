using UnityEngine;
using ActiveSaga.BossFight.Core;

namespace ActiveSaga.BossFight.Entities
{
    public class BossController : MonoBehaviour
    {
        public static BossController Instance { get; private set; }

        [SerializeField] private Animator animator;
        [SerializeField] private float maxHP = 1000f;
        
        private float currentHP;

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

            if (animator == null) animator = GetComponent<Animator>();
            currentHP = maxHP;
        }

        private void Start()
        {
            // Initial UI sync
            EventManager.Trigger(new HealthChangedEvent { current = currentHP, max = maxHP, isPlayer = false });
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
            if (currentHP <= 0) return;

            float oldHP = currentHP;
            currentHP -= amount;
            if (currentHP < 0) currentHP = 0;

            Debug.Log($"[BossController] Damage Received: {amount}. HP: {oldHP} -> {currentHP} / {maxHP}");

            EventManager.Trigger(new HealthChangedEvent 
            { 
                current = currentHP, 
                max = maxHP, 
                isPlayer = false 
            });
            
            if (currentHP <= 0)
            {
                OnBossDefeated();
            }
        }

        private void OnBossDefeated()
        {
            Debug.Log("<color=red>[BossController] Boss Defeated!</color>");
            PlayAnimation("Die");
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
