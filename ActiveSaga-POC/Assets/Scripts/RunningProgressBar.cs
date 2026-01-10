using UnityEngine;
using UnityEngine.UI;

public class RunningProgressBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image fillImage;         // The yellow bar image
    [SerializeField] private RectTransform playerIcon; // The running man icon
    [SerializeField] private RectTransform backgroundBar; // The empty bar (to know width)

    private float barWidth;

    private void Awake()
    {
        // Calculate the total width of the bar in pixels
        if (backgroundBar != null)
        {
            barWidth = backgroundBar.rect.width;
        }
    }

    // This method updates the visuals based on percentage (0.0 to 1.0)
   // This method updates the visuals based on percentage (0.0 to 1.0)
    public void UpdateVisuals(float progressData)
    {
        // FIX: Clamp the value between 0 and 1 so the UI doesn't break 
        // even if the player runs 200% of the distance.
        float clampedProgress = Mathf.Clamp01(progressData);

        // 1. Update the yellow fill
        if (fillImage != null)
        {
            fillImage.fillAmount = clampedProgress;
        }

        // 2. Move the icon along the bar
        if (playerIcon != null)
        {
            // Calculate X position based on the CLAMPED value
            float newX = (barWidth * clampedProgress) - (barWidth / 2f);
            
            Vector2 newPos = playerIcon.anchoredPosition;
            newPos.x = newX;
            playerIcon.anchoredPosition = newPos;
        }
    }
}