using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class MainUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI txtWelcome;
    public TextMeshProUGUI txtLvl;
    public TextMeshProUGUI txtXP;
    public TextMeshProUGUI txtDistance;
    public TextMeshProUGUI txtTime;
    public TextMeshProUGUI txtCoins;

    [Header("Experience Bar")]
    public Image xpFillImage;

    private readonly int[] xpThresholds = { 0, 500, 1500, 3000, 5000, 8000, 12000, 18000, 25000, 35000 };

    private string statsUrl = "http://localhost:3000/api/player/me";

    void Start()
    {
        StartCoroutine(LoadPlayerData());
    }

    IEnumerator LoadPlayerData()
    {
        string token = PlayerPrefs.GetString("AuthToken");

        if (string.IsNullOrEmpty(token))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Login");
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequest.Get(statsUrl))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                PlayerStats stats = JsonUtility.FromJson<PlayerStats>(request.downloadHandler.text);
                UpdateUI(stats);
            }
            else
            {
                Debug.LogError("❌ Error fetching stats: " + request.error);
            }
        }
    }

    void UpdateUI(PlayerStats stats)
    {
        //update name and level
        txtWelcome.text = "Welcome " + stats.firstName;
        txtLvl.text = "level : " + stats.level;

        //update xp bar and text
        UpdateXPBar(stats.xp, stats.level);

        //update coins
        if (txtCoins != null)
        {
            txtCoins.text = stats.coins.ToString();
        }

        //update distance and time
        txtDistance.text = stats.totalDistanceRun.ToString("F1") + " m";

        int totalSeconds = Mathf.FloorToInt(stats.totalTimeInGame);
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;
        if (txtTime != null)
        {
            txtTime.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }
    }

    void UpdateXPBar(int currentXP, int currentLevel)
    {
        int currentLevelThreshold = xpThresholds[Mathf.Clamp(currentLevel - 1, 0, xpThresholds.Length - 1)];
        int nextLevelThreshold = xpThresholds[Mathf.Clamp(currentLevel, 0, xpThresholds.Length - 1)];

        int xpInThisLevel = currentXP - currentLevelThreshold;
        int xpRequiredForNextLevel = nextLevelThreshold - currentLevelThreshold;

        if (xpRequiredForNextLevel <= 0) xpRequiredForNextLevel = 1;

        txtXP.text = $"xp : {xpInThisLevel} / {xpRequiredForNextLevel}";

        if (xpFillImage != null)
        {
            float fillPercentage = (float)xpInThisLevel / xpRequiredForNextLevel;
            xpFillImage.fillAmount = Mathf.Clamp01(fillPercentage);
        }
    }
}