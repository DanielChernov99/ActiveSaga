using UnityEngine;
using UnityEngine.UI;

public class RunningProgressBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fillImage;         // The yellow bar image
    [SerializeField] private RectTransform playerIcon; // The running man icon
    [SerializeField] private RectTransform backgroundBar; // The empty bar (to know width)
    [SerializeField] private GameObject flagIcon;  // The finish line flag icon

    private float barWidth;
    private float iconWidthHalf;

    private void Awake()
    {
        // Calculate the total width of the bar in pixels
        if (backgroundBar != null)
        {
            barWidth = backgroundBar.rect.width;
        }

        // Calculate half the width of the icon for clamping logic
        if (playerIcon != null)
        {
            iconWidthHalf = playerIcon.rect.width / 2f;
        }
    }

    // This method updates the visuals based on percentage (0.0 to 1.0)
    public void UpdateVisuals(float progressData)
    {
        // 1. Clamp value between 0 and 1 to prevent UI overflow
        float clampedProgress = Mathf.Clamp01(progressData);

        // 2. Update the fill amount
        if (fillImage != null)
        {
            fillImage.fillAmount = clampedProgress;
        }

        // 3. Move the icon smoothly along the bar
        if (playerIcon != null)
        {
            // Step A: Calculate the ideal position (exactly on the fill edge)
            // Center anchor: 0 is the middle, -width/2 is left, width/2 is right
            float idealX = (barWidth * clampedProgress) - (barWidth / 2f);

            // Step B: Calculate bounds so the icon stays inside the bar visuals
            float maxX = (barWidth / 2f) - iconWidthHalf;
            float minX = (-barWidth / 2f) + iconWidthHalf;

            // Step C: Clamp the ideal position within the bounds
            // This ensures the icon rides the green bar but stops before exiting the frame
            float finalX = Mathf.Clamp(idealX, minX, maxX);

            Vector2 newPos = playerIcon.anchoredPosition;
            newPos.x = finalX;
            playerIcon.anchoredPosition = newPos;
        }

        // 4. Update Flag visibility
        if (flagIcon != null)
        {
            bool isFinished = clampedProgress >= 0.99f;
            // Show flag while running, hide it when finished
            flagIcon.SetActive(!isFinished);
        }
    }
}