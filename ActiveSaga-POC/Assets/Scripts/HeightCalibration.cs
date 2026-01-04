using UnityEngine;
using System.Collections;

public class HeightCalibration : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BodyTracker bodyTracker;

    [Header("Calibration Data")]
    [SerializeField] private float baseHeight = 0f;

    public bool IsCalibrated { get; private set; } = false;
    public float BaseHeight => baseHeight;

    private const string HeightKey = "BaseHeightValue";

    void Start()
    {
        // Wait 3 seconds before calibrating to allow the player to stand up straight
        StartCoroutine(CalibrateWithDelay());
    }

    private IEnumerator CalibrateWithDelay()
    {
        Debug.Log("Calibration starting in 3 seconds... Please stand straight.");
        
        yield return new WaitForSeconds(3f);

        Calibrate();
    }

    public void Calibrate()
    {
        if (bodyTracker == null)
        {
            Debug.LogError("HeightCalibration: BodyTracker not assigned!");
            return;
        }

        baseHeight = bodyTracker.HeadPosition.y + 0.08f; // Adding offset for better accuracy
        IsCalibrated = true;

        PlayerPrefs.SetFloat(HeightKey, baseHeight);
        PlayerPrefs.Save();

        Debug.Log($"Calibration Complete! New Baseline Head Height: {baseHeight:F2} meters.");
    }

    public float GetHeightDelta()
    {
        if (!IsCalibrated || bodyTracker == null) return 0f;

        return bodyTracker.HeadPosition.y - baseHeight;
    }

    [ContextMenu("Reset Calibration")]
    public void ResetCalibration()
    {
        PlayerPrefs.DeleteKey(HeightKey);
        baseHeight = 0;
        IsCalibrated = false;
        Debug.Log("Calibration reset.");
    }
}