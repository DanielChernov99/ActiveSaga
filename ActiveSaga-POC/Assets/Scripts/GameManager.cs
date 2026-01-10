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

    [Header("Game Settings")]
    [Tooltip("Distance in meters required to fill the progress bar completely.")]
    public float levelTargetDistance = 50f; // Lowered default for testing
    
    [SerializeField] private float runSpeedMultiplier = 8.0f; // Increased default speed

    [Header("Bonus Settings")]
    [Tooltip("Distance added immediately when jumping")]
    [SerializeField] private float jumpDistanceBonus = 5.0f;
    [Tooltip("Distance added immediately when completing a squat")]
    [SerializeField] private float squatDistanceBonus = 2.5f;

    [Header("Debug Info")]
    // Changed from Property to Field so you can see it in Inspector
    public float currentDistance; 
    public int totalJumps;
    public int totalSquats;

    // Flags
    private bool isGameActive = false;

    // Event: Sends (CurrentDistance, TotalJumps, TotalSquats)
    public event Action<float, int, int> OnStatsUpdated;

    private void Start()
    {
        // 1. Check if we have a calibration component assigned
        if (heightCalibration != null)
        {
            Debug.Log("GameManager: Waiting for Calibration...");
            // Subscribe to the completion event
            heightCalibration.OnCalibrationComplete += StartGame;
        }
        else
        {
            // Fallback: If no calibration is assigned, start immediately (good for testing)
            Debug.LogWarning("GameManager: No HeightCalibration assigned! Starting immediately.");
            StartGame();
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (heightCalibration != null) 
            heightCalibration.OnCalibrationComplete -= StartGame;
        
        if (runAnalyzer != null) runAnalyzer.OnRunIntensity -= HandleRun;
        if (jumpAnalyzer != null) jumpAnalyzer.OnJump -= HandleJump;
        if (squatAnalyzer != null) squatAnalyzer.OnSquatCompleted -= HandleSquat;
    }

    private void StartGame()
    {
        Debug.Log("GameManager: Calibration Done. Game Started!");
        isGameActive = true;

        // 2. Subscribe to movement events only AFTER calibration
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
        
        // Add instant bonus distance for jumping
        AddDistance(jumpDistanceBonus);
    }

    private void HandleSquat()
    {
        if (!isGameActive) return;
        
        totalSquats++;

        // Add instant bonus distance for squatting
        AddDistance(squatDistanceBonus);
    }

    // Centralized method to update distance and UI
    private void AddDistance(float amount)
    {
        currentDistance += amount;

        // Removed the code that clamps currentDistance to levelTargetDistance.
        // Now it can go higher than 100 (or whatever the target is).
        
        if (currentDistance >= levelTargetDistance)
        {
            // Logic for level complete can happen here, 
            // but we allow currentDistance to keep growing.
        }

        NotifyUI();
    }

    private void NotifyUI()
    {
        OnStatsUpdated?.Invoke(currentDistance, totalJumps, totalSquats);
    }
}