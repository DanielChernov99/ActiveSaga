using UnityEngine;
using ActiveSaga.BossFight.Combat;

namespace ActiveSaga.BossFight.Core
{
    [RequireComponent(typeof(Collider))]
    public class BonusOrbController : MonoBehaviour
    {
        [Header("Hit Settings")]
        [SerializeField] private float requiredHitSpeed = 0.8f;
        [SerializeField] private float lifetime = 4f;

        [Header("Feedback")]
        [SerializeField] private string collectMessage = "Energy Bonus +5";
        [SerializeField] private float feedbackDuration = 1.2f;
        [SerializeField] private AudioClip collectSfx;
        [SerializeField] private float collectSfxVolume = 0.8f;

        [Header("Visual")]
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float pulseAmount = 0.08f;
        [SerializeField] private float pulseSpeed = 5f;

        private Collider orbCollider;
        private Vector3 originalScale;
        private bool collected;

        private void Awake()
        {
            orbCollider = GetComponent<Collider>();
            orbCollider.isTrigger = true;

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }

            rb.useGravity = false;
            rb.isKinematic = true;

            originalScale = transform.localScale;
        }

        private void OnEnable()
        {
            collected = false;
            CancelInvoke(nameof(Expire));
            Invoke(nameof(Expire), lifetime);
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(Expire));
        }

        private void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

            if (pulseAmount > 0f)
            {
                float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
                transform.localScale = originalScale * pulse;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (collected)
            {
                return;
            }

            WeaponController weapon = other.GetComponentInParent<WeaponController>();
            if (weapon == null)
            {
                return;
            }

            if (!weapon.IsHitValid(requiredHitSpeed))
            {
                return;
            }

            Collect();
        }

        private void Collect()
        {
            collected = true;

            if (orbCollider != null)
            {
                orbCollider.enabled = false;
            }

            if (collectSfx != null)
            {
                AudioSource.PlayClipAtPoint(collectSfx, transform.position, collectSfxVolume);
            }

            EventManager.Trigger(new FeedbackEvent
            {
                message = collectMessage,
                duration = feedbackDuration
            });

            Destroy(gameObject);
        }

        private void Expire()
        {
            if (!collected)
            {
                Destroy(gameObject);
            }
        }
    }
}
