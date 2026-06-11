using UnityEngine;
using UnityEngine.UI;

public class UIButtonSoundInstaller : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool includeInactiveButtons = true;
    [SerializeField] private bool installOnStart = true;

    private void Start()
    {
        if (installOnStart)
        {
            InstallButtonSounds();
        }
    }

    [ContextMenu("Install Button Sounds")]
    public void InstallButtonSounds()
    {
        Button[] buttons = GetComponentsInChildren<Button>(includeInactiveButtons);

        int installedCount = 0;

        foreach (Button button in buttons)
        {
            if (button == null)
            {
                continue;
            }

            UIButtonSound existingSound = button.GetComponent<UIButtonSound>();

            if (existingSound != null)
            {
                continue;
            }

            button.gameObject.AddComponent<UIButtonSound>();
            installedCount++;
        }

        Debug.Log(
            "UIButtonSoundInstaller: installed sounds on " +
            installedCount +
            " buttons under " +
            gameObject.name
        );
    }
}