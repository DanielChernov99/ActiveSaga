using UnityEngine;
using System.Collections;
using System; // Required for Action

public class HeightCalibration : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BodyTracker bodyTracker;

    [Header("Settings")]
    [SerializeField] private float calibrationDelay = 3.0f;

    // Data
    public float BaseHeight { get; private set; }
    public bool IsCalibrated { get; private set; }

    // --- Event ---
    // Fires when calibration is finished so the Game Manager knows the player is ready
    public event Action OnCalibrationComplete;

    private void Start()
    {
        StartCoroutine(CalibrateAfterDelay());
    }

    private IEnumerator CalibrateAfterDelay()
    {
        Debug.Log($"HeightCalibration: Stand straight... Calibrating in {calibrationDelay} seconds.");
        
        yield return new WaitForSeconds(calibrationDelay);
        
        Calibrate();
    }

    public void Calibrate()
    {
        if (bodyTracker == null)
        {
            Debug.LogWarning("HeightCalibration: BodyTracker is missing!");
            return;
        }

        // Set the base height
        BaseHeight = bodyTracker.HeadPosition.y;
        IsCalibrated = true;

        Debug.Log($"Calibration Complete. New Base Height: {BaseHeight}");

        // Notify listeners (GameManager, UI, etc.)
        OnCalibrationComplete?.Invoke();
    }
}