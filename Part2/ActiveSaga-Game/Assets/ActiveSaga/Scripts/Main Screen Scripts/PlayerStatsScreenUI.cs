using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class PlayerStatsApiResponse
{
    public PlayerStatsProfile profile;
    public PlayerLevelInfo levelInfo;
}

[System.Serializable]
public class PlayerStatsProfile
{
    public string firstName;
    public int level;
    public int xp;
    public float totalDistanceRun;
    public float totalTimeInGame;
}

[System.Serializable]
public class PlayerLevelInfo
{
    public int level;
    public int currentLevelXp;
    public int nextLevelXp;
    public int xpIntoCurrentLevel;
    public int xpNeededForNextLevel;
}

public class PlayerStatsScreenUI : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI txtLevel;
    [SerializeField] private TextMeshProUGUI txtXP;
    [SerializeField] private TextMeshProUGUI txtDistance;
    [SerializeField] private TextMeshProUGUI txtTime;

    [Header("XP Bar")]
    [SerializeField] private Image xpFillImage;

    [Header("Server")]
    [SerializeField] private string statsUrl = "http://localhost:3000/api/player/me";

    private Coroutine loadRoutine;

    private void OnEnable()
    {
        Refresh();
    }

    private void OnDisable()
    {
        if (loadRoutine != null)
        {
            StopCoroutine(loadRoutine);
            loadRoutine = null;
        }
    }

    public void Refresh()
    {
        if (loadRoutine != null)
        {
            StopCoroutine(loadRoutine);
        }

        loadRoutine = StartCoroutine(LoadPlayerStats());
    }

    private IEnumerator LoadPlayerStats()
    {
        string token = PlayerPrefs.GetString("AuthToken");

        if (string.IsNullOrEmpty(token))
        {
            Debug.LogError("No AuthToken found. Cannot load player stats.");
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequest.Get(statsUrl))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error fetching player stats: " + request.error);
                Debug.LogError("Server response: " + request.downloadHandler.text);
                yield break;
            }

            string json = request.downloadHandler.text;

            PlayerStatsApiResponse response = JsonUtility.FromJson<PlayerStatsApiResponse>(json);

            if (response == null || response.profile == null)
            {
                Debug.LogError("Could not parse player stats response.");
                Debug.LogError("JSON was: " + json);
                yield break;
            }

            UpdateUI(response.profile, response.levelInfo);
        }
    }

    private void UpdateUI(PlayerStatsProfile profile, PlayerLevelInfo levelInfo)
    {
        int levelToShow = profile.level;

        if (levelInfo != null && levelInfo.level > 0)
        {
            levelToShow = levelInfo.level;
        }

        if (txtLevel != null)
        {
            txtLevel.text = "level : " + levelToShow;
        }

        UpdateXP(profile.xp, levelInfo);

        if (txtDistance != null)
        {
            txtDistance.text = "Total\nDistance :\n" + FormatDistance(profile.totalDistanceRun);
        }

        if (txtTime != null)
        {
            txtTime.text = "Total Active\nTime: " + FormatTime(profile.totalTimeInGame);
        }
    }

    private void UpdateXP(int totalXp, PlayerLevelInfo levelInfo)
    {
        int xpIntoCurrentLevel = totalXp;
        int xpRequiredForCurrentLevel = 1;

        if (levelInfo != null)
        {
            xpIntoCurrentLevel = Mathf.Max(0, levelInfo.xpIntoCurrentLevel);

            if (levelInfo.nextLevelXp > levelInfo.currentLevelXp)
            {
                xpRequiredForCurrentLevel = levelInfo.nextLevelXp - levelInfo.currentLevelXp;
            }
            else
            {
                xpRequiredForCurrentLevel = Mathf.Max(1, levelInfo.xpIntoCurrentLevel);
            }
        }

        xpIntoCurrentLevel = Mathf.Clamp(xpIntoCurrentLevel, 0, xpRequiredForCurrentLevel);

        if (txtXP != null)
        {
            txtXP.text = "xp : " + xpIntoCurrentLevel + " / " + xpRequiredForCurrentLevel;
        }

        if (xpFillImage != null)
        {
            xpFillImage.fillAmount = (float)xpIntoCurrentLevel / xpRequiredForCurrentLevel;
        }
    }

    private string FormatTime(float totalTimeInGame)
    {
        int totalSeconds = Mathf.FloorToInt(totalTimeInGame);

        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;

        if (hours > 0)
        {
            return hours + "h " + minutes + "m";
        }

        if (minutes > 0)
        {
            return minutes + "m " + seconds + "s";
        }

        return seconds + "s";
    }

    private string FormatDistance(float meters)
    {
        if (meters >= 1000f)
        {
            return (meters / 1000f).ToString("F2") + "km";
        }

        return meters.ToString("F0") + "m";
    }
}