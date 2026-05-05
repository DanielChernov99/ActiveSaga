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

    [Header("Monster Chase")]
    [SerializeField] private MonsterController monsterController;

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

    private float currentSpeed;   // 🔥 חשוב: מקור אמת למהירות
    private bool isGameActive = false;
    private float gameStartTime;

    public event Action<float, int, int, float> OnStatsUpdated;

    private void Start()
    {
        if (heightCalibration != null)
            heightCalibration.OnCalibrationComplete += StartGame;
        else
            StartGame();
    }

    private void OnDestroy()
    {
        if (heightCalibration != null)
            heightCalibration.OnCalibrationComplete -= StartGame;

        if (runAnalyzer != null)
            runAnalyzer.OnRunIntensity -= HandleRun;

        if (jumpAnalyzer != null)
            jumpAnalyzer.OnJump -= HandleJump;

        if (squatAnalyzer != null)
            squatAnalyzer.OnSquatCompleted -= HandleSquat;
    }

    private void StartGame()
    {
        ResetStats();

        isGameActive = true;
        gameStartTime = Time.time;

        if (runAnalyzer != null)
            runAnalyzer.OnRunIntensity += HandleRun;

        if (jumpAnalyzer != null)
            jumpAnalyzer.OnJump += HandleJump;

        if (squatAnalyzer != null)
            squatAnalyzer.OnSquatCompleted += HandleSquat;
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
        if (!isGameActive) return;

        currentSpeed = intensity * scoreSpeedMultiplier;

        currentDistance += currentSpeed * Time.deltaTime;
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

    // 👉 גישה נקייה למהירות עבור מערכות אחרות
    public float GetPlayerSpeed()
    {
        return currentSpeed;
    }

    private void Update()
    {
        if (!isGameActive) return;

        float t = Time.time - gameStartTime;
        OnStatsUpdated?.Invoke(currentDistance, totalJumps, totalSquats, t);
    }
}