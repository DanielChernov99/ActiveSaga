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
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (animator == null) animator = GetComponent<Animator>();
            currentHP = maxHP;
        }

        private void Start()
        {
            EventManager.Trigger(new HealthChangedEvent { current = currentHP, max = maxHP, isPlayer = false });
        }

        public void TakeDamage(float amount)
        {
            currentHP -= amount;
            if (currentHP < 0) currentHP = 0;

            EventManager.Trigger(new HealthChangedEvent { current = currentHP, max = maxHP, isPlayer = false });
            
            // Logic for phase transitions based on HP
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
