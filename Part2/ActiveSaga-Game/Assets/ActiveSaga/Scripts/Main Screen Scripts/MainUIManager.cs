using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.Collections;

// --- Data Structures ---

[System.Serializable]
public class QuestInfo
{
    public string title;
    public string description;
    public int goalValue;
    public string questType;
}

[System.Serializable]
public class DailyQuestEntry
{
    public QuestInfo questId; 
    public bool isCompleted;
    public int currentProgress;
}

// Wrapper class because the /daily-quests route returns { message, quests: [] }
[System.Serializable]
public class QuestResponse
{
    public string message;
    public DailyQuestEntry[] quests;
}

[System.Serializable]
public class FullPlayerProfile
{
    public string firstName;
    public int level;
    public int xp;
    public int coins;
    public float totalDistanceRun;
    public float totalTimeInGame;
    public DailyQuestEntry[] dailyQuests; 
}

public class MainUIManager : MonoBehaviour
{
    [Header("General UI Elements")]
    public TextMeshProUGUI txtWelcome;
    public TextMeshProUGUI txtLvl;
    public TextMeshProUGUI txtXP;
    public TextMeshProUGUI txtDistance;
    public TextMeshProUGUI txtTime;
    public TextMeshProUGUI txtCoins;

    [Header("Experience Bar")]
    public Image xpFillImage;
    private readonly int[] xpThresholds = { 0, 500, 1500, 3000, 5000, 8000, 12000, 18000, 25000, 35000 };

    [Header("Daily Quests UI")]
    public TextMeshProUGUI[] questTexts; 
    public Image[] questMedalImages; 

    [Header("Medal Sprites")]
    public Sprite greyMedal;  
    public Sprite yellowMedal; 

    // URLs
    private string statsUrl = "http://localhost:3000/api/player/me";
    private string dailyQuestsUrl = "http://localhost:3000/api/player/daily-quests";

    void Start()
    {
        // We start by generating/fetching the quests first
        StartCoroutine(InitializeDashboard());
    }

    IEnumerator InitializeDashboard()
    {
        string token = PlayerPrefs.GetString("AuthToken");

        if (string.IsNullOrEmpty(token))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Login");
            yield break;
        }

        // 1. First, call the route that generates quests if they don't exist
        yield return StartCoroutine(FetchDailyQuests(token));

        // 2. Then, fetch the rest of the player stats
        yield return StartCoroutine(LoadPlayerData(token));
    }

    IEnumerator FetchDailyQuests(string token)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(dailyQuestsUrl))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Parse using the wrapper class since the server returns an object with a 'quests' array
                QuestResponse response = JsonUtility.FromJson<QuestResponse>(request.downloadHandler.text);
                UpdateQuestUI(response.quests);
            }
            else
            {
                Debug.LogError("❌ Error generating quests: " + request.error);
            }
        }
    }

    IEnumerator LoadPlayerData(string token)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(statsUrl))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                FullPlayerProfile stats = JsonUtility.FromJson<FullPlayerProfile>(request.downloadHandler.text);
                UpdateGeneralUI(stats);
            }
            else
            {
                Debug.LogError("❌ Error fetching stats: " + request.error);
            }
        }
    }

    void UpdateGeneralUI(FullPlayerProfile stats)
    {
        txtWelcome.text = "Welcome " + stats.firstName;
        txtLvl.text = "Level: " + stats.level;
        UpdateXPBar(stats.xp, stats.level);

        if (txtCoins != null) txtCoins.text = stats.coins.ToString();
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

    void UpdateQuestUI(DailyQuestEntry[] quests)
    {
        if (quests == null) return;

        for (int i = 0; i < quests.Length && i < questTexts.Length; i++)
        {
            var entry = quests[i];
            var quest = entry.questId;

            if (quest != null)
            {
                questTexts[i].text = $"{quest.title}\n({entry.currentProgress}/{quest.goalValue})";
            }

            if (questMedalImages != null && i < questMedalImages.Length && questMedalImages[i] != null)
            {
                questMedalImages[i].sprite = entry.isCompleted ? yellowMedal : greyMedal;
            }
        }
    }

    void UpdateXPBar(int currentXP, int currentLevel)
    {
        int currentLevelThreshold = xpThresholds[Mathf.Clamp(currentLevel - 1, 0, xpThresholds.Length - 1)];
        int nextLevelThreshold = xpThresholds[Mathf.Clamp(currentLevel, 0, xpThresholds.Length - 1)];

        int xpInThisLevel = currentXP - currentLevelThreshold;
        int xpRequiredForNextLevel = nextLevelThreshold - currentLevelThreshold;

        if (xpRequiredForNextLevel <= 0) xpRequiredForNextLevel = 1;

        txtXP.text = $"XP: {xpInThisLevel} / {xpRequiredForNextLevel}";

        if (xpFillImage != null)
        {
            float fillPercentage = (float)xpInThisLevel / xpRequiredForNextLevel;
            xpFillImage.fillAmount = Mathf.Clamp01(fillPercentage);
        }
    }

    public void OnLogoutButtonClicked()
    {
        PlayerPrefs.DeleteKey("AuthToken");
        PlayerPrefs.Save();
        Debug.Log("Logged out. Token deleted.");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Login"); 
    }
}

