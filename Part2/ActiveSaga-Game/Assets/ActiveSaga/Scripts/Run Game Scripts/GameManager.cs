using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    [Header("Analyzers References")]
    [SerializeField] private RunAnalyzer runAnalyzer;
    [SerializeField] private JumpAnalyzer jumpAnalyzer;
    [SerializeField] private SquatAnalyzer squatAnalyzer;

    [Header("Calibration & UI")]
    [SerializeField] private HeightCalibration heightCalibration;
    [SerializeField] private UIManager uiManager;

    [Header("Level Goals")]
    public float levelTargetDistance = 100f;
    public int goalJumps = 10;
    public int goalSquats = 5;

    [Header("Game Settings")]
    [Tooltip("Score points per second of running at max intensity")]
    [SerializeField] private float scoreSpeedMultiplier = 8.0f;

    [Header("Runtime Stats (Read Only)")]
    public float currentDistance;
    public int totalJumps;
    public int totalSquats;

    // Internal State
    private bool isGameActive = false;
    private float gameStartTime;

    // Event: Sends (Distance, Jumps, Squats, Time)
    public event Action<float, int, int, float> OnStatsUpdated;

    private void Start()
    {
        // 1. Wait for calibration if it exists
        if (heightCalibration != null)
        {
            Debug.Log("GameManager: Waiting for Calibration...");
            heightCalibration.OnCalibrationComplete += StartGame;
        }
        else
        {
            Debug.LogWarning("GameManager: No HeightCalibration assigned! Starting immediately.");
            StartGame();
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (heightCalibration != null) heightCalibration.OnCalibrationComplete -= StartGame;
        
        if (runAnalyzer != null) runAnalyzer.OnRunIntensity -= HandleRun;
        if (jumpAnalyzer != null) jumpAnalyzer.OnJump -= HandleJump;
        if (squatAnalyzer != null) squatAnalyzer.OnSquatCompleted -= HandleSquat;
    }

    private void Update()
    {
        // Continuous UI update for the timer
        if (isGameActive)
        {
            NotifyUI();
        }
    }

    private void StartGame()
    {
        Debug.Log("GameManager: Game Started!");
        
        ResetStats();
        isGameActive = true;
        gameStartTime = Time.time;

        // 2. Subscribe to movement events
        if (runAnalyzer != null) runAnalyzer.OnRunIntensity += HandleRun;
        if (jumpAnalyzer != null) jumpAnalyzer.OnJump += HandleJump;
        if (squatAnalyzer != null) squatAnalyzer.OnSquatCompleted += HandleSquat;

        // 3. Initial UI Update
        NotifyUI();
    }

    private void ResetStats()
    {
        currentDistance = 0f;
        totalJumps = 0;
        totalSquats = 0;
    }

    // --- Event Handlers ---

    private void HandleRun(float intensity)
    {
        if (!isGameActive) return;

        // Calculate score distance (Decoupled from physical movement)
        float distanceStep = intensity * scoreSpeedMultiplier * Time.deltaTime;
        currentDistance += distanceStep;
        
        // Note: No NotifyUI() here because Update() handles it every frame anyway
    }

    private void HandleJump()
    {
        if (!isGameActive) return;
        totalJumps++;
    }

    private void HandleSquat()
    {
        if (!isGameActive) return;
        totalSquats++;
    }

    // --- UI Helper ---

    private void NotifyUI()
    {
        float timeElapsed = isGameActive ? (Time.time - gameStartTime) : 0f;
        OnStatsUpdated?.Invoke(currentDistance, totalJumps, totalSquats, timeElapsed);
    }
}