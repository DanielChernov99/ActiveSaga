using UnityEngine;
using System;
using ActiveSaga.Common.GameSession;
using ActiveSaga.RunGame;

public class GameManager : MonoBehaviour
{
    [Header("Analyzers References")]
    [SerializeField] private RunAnalyzer runAnalyzer;
    [SerializeField] private JumpAnalyzer jumpAnalyzer;
    [SerializeField] private SquatAnalyzer squatAnalyzer;

    [Header("Calibration & UI")]
    [SerializeField] private HeightCalibration heightCalibration;
    [SerializeField] private UIManager uiManager;

    [Header("Player Collision")]
    [SerializeField] private PlayerCollisionHandler playerCollisionHandler;

    [Header("Monster Chase")]
    [SerializeField] private MonsterController monsterController;

    [Header("ActiveSaga Session System")]
    [SerializeField] private GameSessionManager gameSessionManager;
    [SerializeField] private RunGameStatsTracker runGameStatsTracker;

    [Header("Game Settings")]
    [SerializeField] private float scoreSpeedMultiplier = 8f;

    [Header("End Game Behavior")]
    [SerializeField] private bool stopMusicWhenGameEnds = true;

    [Header("Level Goals")]
    public float levelTargetDistance = 100f;
    public int goalJumps = 10;
    public int goalSquats = 5;

    [Header("Stats")]
    public float currentDistance;
    public int totalJumps;
    public int totalSquats;

    [Header("Stun Settings")]
    [SerializeField] private float stunDuration = 2f;

    private float currentSpeed;
    private bool isGameActive = false;
    private float gameStartTime;
    private float stunnedUntilTime = -1f;

    public bool IsGameActive => isGameActive;

    public event Action<float, int, int, float> OnStatsUpdated;

    private void Start()
    {
        SubscribeToEvents();

        if (heightCalibration != null)
        {
            heightCalibration.OnCalibrationComplete += StartGame;
        }
        else
        {
            StartGame();
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();

        if (heightCalibration != null)
        {
            heightCalibration.OnCalibrationComplete -= StartGame;
        }
    }

    private void SubscribeToEvents()
    {
        if (runAnalyzer != null)
        {
            runAnalyzer.OnRunIntensity += HandleRun;
        }

        if (jumpAnalyzer != null)
        {
            jumpAnalyzer.OnJump += HandleJump;
        }

        if (squatAnalyzer != null)
        {
            squatAnalyzer.OnSquatCompleted += HandleSquat;
        }

        if (playerCollisionHandler != null)
        {
            playerCollisionHandler.OnObstacleCrash += HandleObstacleCrash;
        }
        else
        {
            Debug.LogError("[GameManager] playerCollisionHandler is NULL. Obstacle crashes will not work.");
        }

        if (monsterController != null)
        {
            monsterController.OnMonsterCaughtPlayer += HandleMonsterCaughtPlayer;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (runAnalyzer != null)
        {
            runAnalyzer.OnRunIntensity -= HandleRun;
        }

        if (jumpAnalyzer != null)
        {
            jumpAnalyzer.OnJump -= HandleJump;
        }

        if (squatAnalyzer != null)
        {
            squatAnalyzer.OnSquatCompleted -= HandleSquat;
        }

        if (playerCollisionHandler != null)
        {
            playerCollisionHandler.OnObstacleCrash -= HandleObstacleCrash;
        }

        if (monsterController != null)
        {
            monsterController.OnMonsterCaughtPlayer -= HandleMonsterCaughtPlayer;
        }
    }

    private void StartGame()
    {
        ResetStats();

        isGameActive = true;
        gameStartTime = Time.time;

        if (gameSessionManager != null)
        {
            gameSessionManager.StartSession();
            gameSessionManager.StartGameplayTimerIfNeeded();
        }
        else
        {
            Debug.LogError("GameManager: Missing GameSessionManager reference.");
        }

        if (runGameStatsTracker != null)
        {
            runGameStatsTracker.ResetStats();
        }
        else
        {
            Debug.LogError("GameManager: Missing RunGameStatsTracker reference.");
        }

        if (monsterController != null)
        {
            monsterController.BeginChase();
        }

        Debug.Log("Run Game Started");
    }

    private void ResetStats()
    {
        currentDistance = 0f;
        totalJumps = 0;
        totalSquats = 0;
        currentSpeed = 0f;
        stunnedUntilTime = -1f;
    }

    private void HandleRun(float intensity)
    {
        if (!isGameActive)
        {
            currentSpeed = 0f;
            return;
        }

        if (IsPlayerStunned())
        {
            currentSpeed = 0f;
            return;
        }

        currentSpeed = intensity * scoreSpeedMultiplier;
        currentDistance += currentSpeed * Time.deltaTime;

        if (runGameStatsTracker != null)
        {
            runGameStatsTracker.SetDistance(currentDistance);
        }
    }

    private void HandleJump()
    {
        if (!isGameActive)
        {
            return;
        }

        totalJumps++;

        if (runGameStatsTracker != null)
        {
            runGameStatsTracker.AddJump();
        }
    }

    private void HandleSquat()
    {
        if (!isGameActive)
        {
            return;
        }

        totalSquats++;

        if (runGameStatsTracker != null)
        {
            runGameStatsTracker.AddSquat();
        }
    }

    private void HandleObstacleCrash()
    {
        if (!isGameActive)
        {
            Debug.LogWarning("[GameManager] Crash ignored because game is not active.");
            return;
        }

        Debug.Log("GameManager received obstacle crash. Player stunned.");

        stunnedUntilTime = Time.time + stunDuration;
        currentSpeed = 0f;

        if (runGameStatsTracker != null)
        {
            runGameStatsTracker.AddObstacleCrash();
        }
    }

    private void HandleMonsterCaughtPlayer()
    {
        if (!isGameActive)
        {
            return;
        }

        EndRunGame(GameEndReason.GameOver);
    }

    public void EndRunGame(GameEndReason endReason)
    {
        if (!isGameActive)
        {
            return;
        }

        isGameActive = false;
        currentSpeed = 0f;
        stunnedUntilTime = -1f;

        if (monsterController != null)
        {
            monsterController.StopChase();
        }

        if (stopMusicWhenGameEnds && ActiveSagaAudioManager.Instance != null)
        {
            ActiveSagaAudioManager.Instance.StopMusic();
        }

        OnStatsUpdated?.Invoke(currentDistance, totalJumps, totalSquats, Time.time - gameStartTime);

        if (gameSessionManager != null)
        {
            gameSessionManager.EndGame(endReason);
        }
        else
        {
            Debug.LogError("GameManager: Cannot end game because GameSessionManager is missing.");
        }
    }

    public float GetPlayerSpeed()
    {
        if (!isGameActive || IsPlayerStunned())
        {
            return 0f;
        }

        return currentSpeed;
    }

    private void Update()
    {
        if (!isGameActive)
        {
            currentSpeed = 0f;
            return;
        }

        float elapsedTime = Time.time - gameStartTime;

        OnStatsUpdated?.Invoke(currentDistance, totalJumps, totalSquats, elapsedTime);

        if (runGameStatsTracker != null)
        {
            runGameStatsTracker.SetDistance(currentDistance);
        }
    }

    public bool IsPlayerStunned()
    {
        return Time.time < stunnedUntilTime;
    }
}