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

    // --- New Modular Goals Section ---
    [Header("Level Goals")]
    public float levelTargetDistance = 50f; // Existing logic
    public int goalJumps = 10;              // New: Goal for jumps
    public int goalSquats = 5;              // New: Goal for squats

    [Header("Game Settings")]
    [SerializeField] private float runSpeedMultiplier = 8.0f; 

    [Header("Debug Info")]
    public float currentDistance; 
    public int totalJumps;
    public int totalSquats;

    // Flags & Time
    private bool isGameActive = false;
    private float gameStartTime; // New: To track time

    // Event: Sends (Distance, Jumps, Squats, Time) -> Added Time at the end
    public event Action<float, int, int, float> OnStatsUpdated;

    private void Start()
    {
        // 1. Check if we have a calibration component assigned
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

    private void Update()
    {
        // New: Continuous UI update for the timer (so seconds tick even if standing still)
        if (isGameActive)
        {
            NotifyUI();
        }
    }

    private void OnDestroy()
    {
        if (heightCalibration != null) heightCalibration.OnCalibrationComplete -= StartGame;
        if (runAnalyzer != null) runAnalyzer.OnRunIntensity -= HandleRun;
        if (jumpAnalyzer != null) jumpAnalyzer.OnJump -= HandleJump;
        if (squatAnalyzer != null) squatAnalyzer.OnSquatCompleted -= HandleSquat;
    }

    private void StartGame()
    {
        Debug.Log("GameManager: Calibration Done. Game Started!");
        isGameActive = true;
        gameStartTime = Time.time; // New: Reset timer

        // 2. Subscribe to movement events
        if (runAnalyzer != null) runAnalyzer.OnRunIntensity += HandleRun;
        if (jumpAnalyzer != null) jumpAnalyzer.OnJump += HandleJump;
        if (squatAnalyzer != null) squatAnalyzer.OnSquatCompleted += HandleSquat;

        // 3. Reset UI
        NotifyUI();
    }

    private void HandleRun(float intensity)
    {
        if (!isGameActive) return;

        // Calculate distance added this frame
        float distanceStep = intensity * runSpeedMultiplier * Time.deltaTime;
        AddDistance(distanceStep);
    }

    private void HandleJump()
    {
        if (!isGameActive) return;
        totalJumps++;
        // Note: Logic preserved (no distance added on jump)
        NotifyUI();
    }

    private void HandleSquat()
    {
        if (!isGameActive) return;
        totalSquats++;
        // Note: Logic preserved (no distance added on squat)
        NotifyUI();
    }

    private void AddDistance(float amount)
    {
        currentDistance += amount;  
        // Logic for level complete can happen here
        // (currentDistance >= levelTargetDistance)
    }

    private void NotifyUI()
    {
        // New: Calculate elapsed time
        float timeElapsed = isGameActive ? (Time.time - gameStartTime) : 0f;

        // Updated Event invocation with 4 parameters
        OnStatsUpdated?.Invoke(currentDistance, totalJumps, totalSquats, timeElapsed);
    }
}