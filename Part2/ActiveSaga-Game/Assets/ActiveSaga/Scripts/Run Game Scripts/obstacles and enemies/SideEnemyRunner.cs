using UnityEngine;
using ActiveSaga.RunGame;

public class SideEnemyRunner : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Activation")]
    [SerializeField] private float activationDistance = 90f;
    [SerializeField] private float destroyBehindPlayerDistance = 10f;

    [Header("Kill Settings")]
    [SerializeField] private string weaponTag = "Sword";
    [SerializeField] private float deathDestroyDelay = 1.2f;

    [Header("Death Feedback")]
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private Animator animator;
    [SerializeField] private string deathTrigger = "Death";

    [Header("Optional Animation")]
    [SerializeField] private string walkingBool = "";

    [Header("Game State")]
    [SerializeField] private GameManager gameManager;

    private Transform target;
    private RunGameStatsTracker statsTracker;
    private bool isActivated = false;
    private bool isDying = false;

    public void Initialize(Transform playerTarget, RunGameStatsTracker tracker)
    {
        target = playerTarget;
        statsTracker = tracker;

        isActivated = false;
        isDying = false;

        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        SetWalkingAnimation(false);

        if (target == null)
        {
            Debug.LogError("SideEnemyRunner: Missing player target.");
        }
    }

    private void Update()
    {
        if (target == null || isDying)
        {
            return;
        }

        if (!IsGameActive())
        {
            SetWalkingAnimation(false);
            return;
        }

        float zDistanceFromPlayer = transform.position.z - target.position.z;

        if (!isActivated)
        {
            if (zDistanceFromPlayer > activationDistance)
            {
                return;
            }

            ActivateEnemy();
        }

        if (zDistanceFromPlayer < -destroyBehindPlayerDistance)
        {
            Destroy(gameObject);
            return;
        }

        MoveTowardPlayer();
    }

    private bool IsGameActive()
    {
        return gameManager == null || gameManager.IsGameActive;
    }

    private void ActivateEnemy()
    {
        isActivated = true;
        SetWalkingAnimation(true);

        Debug.Log("SideEnemy activated: " + gameObject.name);
    }

    private void MoveTowardPlayer()
    {
        Vector3 targetPosition = target.position;
        targetPosition.y = transform.position.y;

        Vector3 direction = (targetPosition - transform.position).normalized;

        transform.position += direction * moveSpeed * Time.deltaTime;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDying || !IsGameActive())
        {
            return;
        }

        if (!other.CompareTag(weaponTag))
        {
            return;
        }

        KillEnemy();
    }

    private void KillEnemy()
    {
        isDying = true;
        SetWalkingAnimation(false);

        if (statsTracker != null)
        {
            statsTracker.AddEnemyKill();
        }

        DisableColliders();
        PlayDeathAnimation();
        PlayDeathSound();

        Destroy(gameObject, deathDestroyDelay);
    }

    private void DisableColliders()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();

        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }
    }

    private void PlayDeathAnimation()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator != null && !string.IsNullOrEmpty(deathTrigger))
        {
            animator.SetTrigger(deathTrigger);
        }
    }

    private void PlayDeathSound()
    {
        if (deathSound == null)
        {
            return;
        }

        if (ActiveSagaAudioManager.Instance != null)
        {
            ActiveSagaAudioManager.Instance.PlaySFX(deathSound);
            return;
        }

        AudioSource.PlayClipAtPoint(deathSound, transform.position);
    }

    private void SetWalkingAnimation(bool isWalking)
    {
        if (animator == null || string.IsNullOrEmpty(walkingBool))
        {
            return;
        }

        animator.SetBool(walkingBool, isWalking);
    }
}