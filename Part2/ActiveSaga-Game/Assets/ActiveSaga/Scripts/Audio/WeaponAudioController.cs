using UnityEngine;

public class WeaponAudioController : MonoBehaviour
{
    [Header("Whoosh Settings")]
    public AudioClip whooshSound;
    public float velocityThreshold = 1.2f;
    public float whooshCooldown = 0.35f;

    [Header("Movement Reference")]
    [Tooltip("Drag the Player / XR Origin / object with CharacterController here. This removes body movement from the sword speed.")]
    public Transform movementReference;

    [Header("Optional")]
    public AudioSource audioSource;

    private Vector3 lastRelativePosition;
    private float lastWhooshTime = -999f;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (movementReference == null)
        {
            CharacterController characterController = FindFirstObjectByType<CharacterController>();

            if (characterController != null)
            {
                movementReference = characterController.transform;
            }
        }
    }

    private void OnEnable()
    {
        lastRelativePosition = GetRelativePosition();
        lastWhooshTime = -999f;
    }

    private void FixedUpdate()
    {
        Vector3 currentRelativePosition = GetRelativePosition();
        Vector3 relativeVelocity = (currentRelativePosition - lastRelativePosition) / Time.fixedDeltaTime;

        lastRelativePosition = currentRelativePosition;

        TryPlayWhoosh(relativeVelocity.magnitude);
    }

    private Vector3 GetRelativePosition()
    {
        if (movementReference == null)
        {
            return transform.position;
        }

        return movementReference.InverseTransformPoint(transform.position);
    }

    private void TryPlayWhoosh(float speed)
    {
        if (whooshSound == null)
        {
            return;
        }

        if (speed < velocityThreshold)
        {
            return;
        }

        if (Time.time - lastWhooshTime < whooshCooldown)
        {
            return;
        }

        lastWhooshTime = Time.time;
        PlayWhooshSound();
    }

    private void PlayWhooshSound()
    {
        if (audioSource != null)
        {
            audioSource.PlayOneShot(whooshSound);
            return;
        }

        if (ActiveSagaAudioManager.Instance != null)
        {
            ActiveSagaAudioManager.Instance.PlaySFX(whooshSound);
            return;
        }

        AudioSource.PlayClipAtPoint(whooshSound, transform.position);
    }
}