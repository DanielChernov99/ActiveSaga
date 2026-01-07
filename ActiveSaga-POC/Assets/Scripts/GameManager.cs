using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    [Header("Analyzers References")]
    [SerializeField] private RunAnalyzer runAnalyzer;
    [SerializeField] private JumpAnalyzer jumpAnalyzer;
    [SerializeField] private SquatAnalyzer squatAnalyzer;

    [Header("Game Settings")]
    [Tooltip("Distance in meters required to fill the progress bar completely.")]
    public float levelTargetDistance = 100f; 
    
    [SerializeField] private float runSpeedMultiplier = 4.0f; 

    // Public Data
    public float CurrentDistance { get; private set; }
    public int TotalJumps { get; private set; }
    public int TotalSquats { get; private set; }

    // Event: Sends (CurrentDistance, TotalJumps, TotalSquats)
    public event Action<float, int, int> OnStatsUpdated;

    private void Start()
    {
        // Subscribe to analyzers
        if (runAnalyzer != null) runAnalyzer.OnRunIntensity += HandleRun;
        if (jumpAnalyzer != null) jumpAnalyzer.OnJump += HandleJump;
        if (squatAnalyzer != null) squatAnalyzer.OnSquatCompleted += HandleSquat;
        
        // Initial Update to set UI to 0
        NotifyUI();
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (runAnalyzer != null) runAnalyzer.OnRunIntensity -= HandleRun;
        if (jumpAnalyzer != null) jumpAnalyzer.OnJump -= HandleJump;
        if (squatAnalyzer != null) squatAnalyzer.OnSquatCompleted -= HandleSquat;
    }

    private void HandleRun(float intensity)
    {
        // Calculate distance added this frame
        float distanceStep = intensity * runSpeedMultiplier * Time.deltaTime;
        CurrentDistance += distanceStep;
        
        NotifyUI();
    }

    private void HandleJump()
    {
        TotalJumps++;
        NotifyUI();
    }

    private void HandleSquat()
    {
        TotalSquats++;
        NotifyUI();
    }

    private void NotifyUI()
    {
        OnStatsUpdated?.Invoke(CurrentDistance, TotalJumps, TotalSquats);
    }
}