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
    public void UpdateVisuals(float progressData)
    {
        // 1. Update the yellow fill
        if (fillImage != null)
        {
            fillImage.fillAmount = progressData;
        }

        // 2. Move the icon along the bar
        if (playerIcon != null)
        {
            // Calculate X position: (Width * Percentage) - (Half Width to center it)
            // Assumes the pivot is center (0.5, 0.5)
            float newX = (barWidth * progressData) - (barWidth / 2f);
            
            Vector2 newPos = playerIcon.anchoredPosition;
            newPos.x = newX;
            playerIcon.anchoredPosition = newPos;
        }
    }
}