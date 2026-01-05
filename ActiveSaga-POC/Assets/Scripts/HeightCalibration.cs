using UnityEngine;
using System.Collections;

public class HeightCalibration : MonoBehaviour
{
    [SerializeField] private BodyTracker bodyTracker;

    public float BaseHeight { get; private set; }
    public bool IsCalibrated { get; private set; }

    private void Start()
    {
        // We always force a new calibration sequence to support different players.
        StartCoroutine(CalibrateAfterDelay());
    }

    private IEnumerator CalibrateAfterDelay()
    {
        Debug.Log("HeightCalibration: Stand straight... Calibrating in 3 seconds.");
        
        // Wait for 3 seconds to let the player get into position
        yield return new WaitForSeconds(3f);
        
        Calibrate();
    }

    public void Calibrate()
    {
        if (!bodyTracker)
        {
            Debug.LogWarning("HeightCalibration: BodyTracker is missing!");
            return;
        }

        // Set the base height to the current head height of the specific player
        BaseHeight = bodyTracker.HeadPosition.y;
        IsCalibrated = true;

        Debug.Log($"Calibration Complete. New Base Height: {BaseHeight}");
    }
}