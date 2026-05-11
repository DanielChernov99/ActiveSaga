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

    [Header("Level Goals")]
    public float levelTargetDistance = 100f;
    public int goalJumps = 10;
    public int goalSquats = 5;

    [Header("Stats")]
    public float currentDistance;
    public int totalJumps;
    public int totalSquats;

    private float currentSpeed;
    private bool isGameActive = false;
    private float gameStartTime;

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
            playerCollisionHandler.OnObstacleGraze += HandleObstacleGraze;
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
            playerCollisionHandler.OnObstacleGraze -= HandleObstacleGraze;
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
    }

    private void HandleRun(float intensity)
    {
        if (!isGameActive)
        {
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
            return;
        }

        Debug.Log("GameManager received obstacle crash.");

        if (runGameStatsTracker != null)
        {
            runGameStatsTracker.AddObstacleCrash();
        }

        /*
        אם בעתיד תרצה שהתנגשות חזקה תסיים משחק:
        EndRunGame(GameEndReason.GameOver);
        */
    }

    private void HandleObstacleGraze()
    {
        if (!isGameActive)
        {
            return;
        }

        Debug.Log("GameManager received obstacle graze.");

        if (runGameStatsTracker != null)
        {
            runGameStatsTracker.AddObstacleGraze();
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

        if (monsterController != null)
        {
            monsterController.StopChase();
        }

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
        return currentSpeed;
    }

    private void Update()
    {
        if (!isGameActive)
        {
            return;
        }

        float t = Time.time - gameStartTime;

        OnStatsUpdated?.Invoke(currentDistance, totalJumps, totalSquats, t);

        if (runGameStatsTracker != null)
        {
            runGameStatsTracker.SetDistance(currentDistance);
        }
    }
}