using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class RunPlayerAudio : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private RunAnalyzer runAnalyzer;
    [SerializeField] private JumpAnalyzer jumpAnalyzer;

    [Header("Running Loop")]
    [SerializeField] private AudioSource runningLoopSource;
    [SerializeField] private AudioClip runningLoopClip;
    [SerializeField] private float minRunIntensityForSound = 0.12f;
    [SerializeField] private float runEventTimeout = 0.25f;

    [Header("Jump Sounds")]
    [SerializeField] private AudioClip jumpStartClip;
    [SerializeField] private AudioClip landingClip;
    [SerializeField] private float minAirTimeForLandingSound = 0.15f;

    private CharacterController characterController;
    private float lastRunIntensity;
    private float lastRunEventTime;
    private bool wasGrounded;
    private bool jumpStarted;
    private float airborneStartTime;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (runningLoopSource == null)
        {
            runningLoopSource = gameObject.AddComponent<AudioSource>();
        }

        runningLoopSource.playOnAwake = false;
        runningLoopSource.loop = true;
        runningLoopSource.spatialBlend = 0f;
        runningLoopSource.clip = runningLoopClip;
    }

    private void OnEnable()
    {
        if (runAnalyzer != null)
        {
            runAnalyzer.OnRunIntensity += HandleRunIntensity;
        }

        if (jumpAnalyzer != null)
        {
            jumpAnalyzer.OnJump += HandleJump;
        }
    }

    private void OnDisable()
    {
        if (runAnalyzer != null)
        {
            runAnalyzer.OnRunIntensity -= HandleRunIntensity;
        }

        if (jumpAnalyzer != null)
        {
            jumpAnalyzer.OnJump -= HandleJump;
        }

        StopRunningLoop();
    }

    private void Start()
    {
        wasGrounded = characterController.isGrounded;
    }

    private void Update()
    {
        UpdateRunningLoop();
        UpdateLandingSound();
    }

    private void HandleRunIntensity(float intensity)
    {
        lastRunIntensity = intensity;
        lastRunEventTime = Time.time;
    }

    private void HandleJump()
    {
        if (!CanPlayPlayerAudio())
        {
            return;
        }

        if (gameManager != null && gameManager.IsPlayerStunned())
        {
            return;
        }

        PlayOneShot(jumpStartClip);

        jumpStarted = true;
        airborneStartTime = Time.time;

        StopRunningLoop();
    }

    private void UpdateRunningLoop()
    {
        if (!CanPlayPlayerAudio())
        {
            StopRunningLoop();
            return;
        }

        if (gameManager != null && gameManager.IsPlayerStunned())
        {
            StopRunningLoop();
            return;
        }

        bool runSignalIsFresh = Time.time - lastRunEventTime <= runEventTimeout;
        bool shouldPlay =
            characterController.isGrounded &&
            runSignalIsFresh &&
            lastRunIntensity >= minRunIntensityForSound;

        if (shouldPlay)
        {
            StartRunningLoop();
        }
        else
        {
            StopRunningLoop();
        }
    }

    private void UpdateLandingSound()
    {
        bool isGrounded = characterController.isGrounded;

        if (!wasGrounded && isGrounded)
        {
            float airTime = Time.time - airborneStartTime;

            if (jumpStarted && airTime >= minAirTimeForLandingSound && CanPlayPlayerAudio())
            {
                PlayOneShot(landingClip);
            }

            jumpStarted = false;
        }

        if (wasGrounded && !isGrounded)
        {
            airborneStartTime = Time.time;
        }

        wasGrounded = isGrounded;
    }

    private bool CanPlayPlayerAudio()
    {
        return gameManager == null || gameManager.IsGameActive;
    }

    private void StartRunningLoop()
    {
        if (runningLoopClip == null || runningLoopSource == null)
        {
            return;
        }

        if (runningLoopSource.clip != runningLoopClip)
        {
            runningLoopSource.clip = runningLoopClip;
        }

        if (!runningLoopSource.isPlaying)
        {
            runningLoopSource.Play();
        }
    }

    private void StopRunningLoop()
    {
        if (runningLoopSource != null && runningLoopSource.isPlaying)
        {
            runningLoopSource.Stop();
        }
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        if (ActiveSagaAudioManager.Instance != null)
        {
            ActiveSagaAudioManager.Instance.PlaySFX(clip);
        }
    }
}