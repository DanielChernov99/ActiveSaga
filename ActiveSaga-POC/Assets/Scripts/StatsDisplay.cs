using UnityEngine;
using TMPro;

public class StatsDisplay : MonoBehaviour
{
    [Header("UI Text References")]
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private TextMeshProUGUI jumpText;
    [SerializeField] private TextMeshProUGUI squatText;
    [SerializeField] private TextMeshProUGUI timeText;

    [Header("Settings")]
    [Tooltip("Extra space between the longest label and the colon.")]
    [SerializeField] private float spacingPadding = 20f; 

    private float calculatedAlignPos = 0f;
    private float targetDist;
    private int targetJumps;
    private int targetSquats;

    private void Start()
    {
        CalculateAlignment();
    }

    private void CalculateAlignment()
    {
        float w1 = GetLabelWidth(distanceText, "Distance");
        float w2 = GetLabelWidth(jumpText, "Jumps");
        float w3 = GetLabelWidth(squatText, "Squats");
        float w4 = GetLabelWidth(timeText, "Time");

        float maxLabelWidth = Mathf.Max(w1, w2, w3, w4);

        calculatedAlignPos = maxLabelWidth + spacingPadding;

    }

    private float GetLabelWidth(TextMeshProUGUI tmpComponent, string label)
    {
        if (tmpComponent == null) return 0f;
        return tmpComponent.GetPreferredValues(label).x;
    }

    public void SetGoals(float dist, int jumps, int squats)
    {
        targetDist = dist;
        targetJumps = jumps;
        targetSquats = squats;
    }

    public void UpdateStats(float currentDist, int currentJumps, int currentSquats, float timeInSeconds)
    {
        string indent = $"<pos={calculatedAlignPos}>"; 

        // 1. Distance
        if (distanceText != null)
        {
            distanceText.text = $"Distance {indent}: {currentDist:0.0} / {targetDist:0} m";
            distanceText.color = (currentDist >= targetDist) ? new Color(0, 0.5f, 0) : Color.black; 
        }

        // 2. Jumps
        if (jumpText != null)
        {
            jumpText.text = $"Jumps {indent}: {currentJumps} / {targetJumps}";
            jumpText.color = (currentJumps >= targetJumps) ? new Color(0, 0.5f, 0) : Color.black;
        }

        // 3. Squats
        if (squatText != null)
        {
            squatText.text = $"Squats {indent}: {currentSquats} / {targetSquats}";
            squatText.color = (currentSquats >= targetSquats) ? new Color(0, 0.5f, 0) : Color.black;
        }

        // 4. Time
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60F);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60F);
            timeText.text = $"Time {indent}: {minutes:00}:{seconds:00}";
        }
    }
}