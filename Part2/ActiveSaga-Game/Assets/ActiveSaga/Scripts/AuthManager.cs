using UnityEngine;
using UnityEngine.Networking;
using TMPro; 
using System.Text;
using System.Collections;
using UnityEngine.SceneManagement;

// --- Request Classes ---
[System.Serializable]
public class LoginRequest
{
    public string identifier;
    public string password;
}

[System.Serializable]
public class RegisterRequest
{
    public string email;
    public string username;
    public string password;
    public string firstName;
    public string lastName;
}

// --- Response Classes ---
[System.Serializable]
public class PlayerStats
{
    public string firstName;
    public string lastName;
    public int level;
    public int xp;
    public int coins;
    public string[] inventory;
}

[System.Serializable]
public class LoginResponse
{
    public string message;
    public string accountId;
    public string username;
    public string token;
    public PlayerStats playerStats;
}

[System.Serializable]
public class RegisterResponse
{
    public string message;
    public string accountId;
    public string token;
}

[System.Serializable]
public class ErrorResponse
{
    public string message;
}


public class AuthManager : MonoBehaviour
{
    private string baseUrl = "http://localhost:3000/api/auth";

    [Header("Error Displays")]
    public TextMeshProUGUI loginErrorText;   
    public TextMeshProUGUI registerErrorText;

    [Header("Login UI Fields")]
    public TMP_InputField loginIdentifierInput; 
    public TMP_InputField loginPasswordInput;

    [Header("Register UI Fields")]
    public TMP_InputField regEmailInput;
    public TMP_InputField regUsernameInput;
    public TMP_InputField regPasswordInput;
    public TMP_InputField regFirstNameInput;
    public TMP_InputField regLastNameInput;


    private void ShowError(string errorMessage, TextMeshProUGUI displayTarget)
    {
        if (displayTarget != null)
        {
            displayTarget.text = errorMessage;
        }
    }

    private void ClearErrors()
    {
        if (loginErrorText != null) loginErrorText.text = "";
        if (registerErrorText != null) registerErrorText.text = "";
    }


    // --- Login Functions ---
    public void OnLoginButtonClicked()
    {
        ClearErrors();

        string identifier = loginIdentifierInput.text;
        string password = loginPasswordInput.text;

        if (string.IsNullOrEmpty(identifier) || string.IsNullOrEmpty(password))
        {
            ShowError("Please fill in all fields.", loginErrorText);
            return;
        }

        StartCoroutine(LoginCoroutine(identifier, password));
    }

    private IEnumerator LoginCoroutine(string identifier, string password)
    {
        LoginRequest loginData = new LoginRequest { identifier = identifier, password = password };
        string jsonData = JsonUtility.ToJson(loginData);

        using (UnityWebRequest request = new UnityWebRequest(baseUrl + "/login", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                string errorMsg = "An error occurred during login.";

                if (!string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    try
                    {
                        ErrorResponse serverError = JsonUtility.FromJson<ErrorResponse>(request.downloadHandler.text);
                        if (serverError != null && !string.IsNullOrEmpty(serverError.message))
                        {
                            errorMsg = serverError.message;
                        }
                    }
                    catch { }
                }

                Debug.LogError($"❌ Login Error: {request.error}\nServer Response: {request.downloadHandler.text}");
                ShowError(errorMsg, loginErrorText);
            }
            else
            {
                Debug.Log("✅ Login Successful!");
                
                LoginResponse responseData = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                
                if (responseData != null && responseData.playerStats != null)
                {
                    Debug.Log($"\n=== Player Profile Info ===\n" +
                              $"Name: {responseData.playerStats.firstName} {responseData.playerStats.lastName}\n" +
                              $"Level: {responseData.playerStats.level}\n" +
                              $"XP: {responseData.playerStats.xp}\n" +
                              $"Coins: {responseData.playerStats.coins}\n" +
                              $"Inventory: {string.Join(", ", responseData.playerStats.inventory)}\n" +
                              $"===========================");
                }

                // Clear the input fields after successful login
                loginIdentifierInput.text = "";
                loginPasswordInput.text = "";

                // Store the auth token for future authenticated requests
                PlayerPrefs.SetString("AuthToken", responseData.token);
                PlayerPrefs.Save();

                // Load the main game scene after successful login
                SceneManager.LoadScene("Main");
            }
        }
    }

    // --- Register Functions ---
    public void OnRegisterButtonClicked()
    {
        ClearErrors();

        string email = regEmailInput.text;
        string username = regUsernameInput.text;
        string password = regPasswordInput.text;
        string firstName = regFirstNameInput.text;
        string lastName = regLastNameInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
        {
            ShowError("Please fill in all required fields.", registerErrorText);
            return;
        }

        if (!IsValidEmail(email))
        {
            ShowError("Please enter a valid email address.", registerErrorText);
            return;
        }

        StartCoroutine(RegisterCoroutine(email, username, password, firstName, lastName));
    }

    private IEnumerator RegisterCoroutine(string email, string username, string password, string firstName, string lastName)
    {
        RegisterRequest regData = new RegisterRequest 
        { 
            email = email, 
            username = username, 
            password = password, 
            firstName = firstName, 
            lastName = lastName 
        };
        string jsonData = JsonUtility.ToJson(regData);

        using (UnityWebRequest request = new UnityWebRequest(baseUrl + "/register", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                string errorMsg = "An error occurred during registration.";
                if (!string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    try
                    {
                        ErrorResponse serverError = JsonUtility.FromJson<ErrorResponse>(request.downloadHandler.text);
                        if (serverError != null && !string.IsNullOrEmpty(serverError.message))
                        {
                            errorMsg = serverError.message;
                        }
                    }
                    catch { }
                }

                Debug.LogError($"❌ Registration Error: {request.error}\nServer Response: {request.downloadHandler.text}");
                ShowError(errorMsg, registerErrorText);
            }
            else
            {
                Debug.Log($"🎮 ✅ New player registered successfully!");
                
                // Clear the input fields after successful registration
                regEmailInput.text = "";
                regUsernameInput.text = "";
                regPasswordInput.text = "";
                regFirstNameInput.text = "";
                regLastNameInput.text = "";

                // Store the auth token for future authenticated requests
                RegisterResponse responseData = JsonUtility.FromJson<RegisterResponse>(request.downloadHandler.text);
                PlayerPrefs.SetString("AuthToken", responseData.token);
                PlayerPrefs.Save();
                
                //Load the main game scene after successful registration
                SceneManager.LoadScene("Main");
            }
        }
    }

    public IEnumerator FetchPlayerStats()
    {
        string token = PlayerPrefs.GetString("AuthToken");
        
        using (UnityWebRequest request = UnityWebRequest.Get("http://localhost:3000/api/player/me"))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("❌ Error fetching stats: " + request.error);
            }
            else
            {
                PlayerStats stats = JsonUtility.FromJson<PlayerStats>(request.downloadHandler.text);
                Debug.Log($"✅ Loaded stats for ActiveSaga UI: Level {stats.level}, XP {stats.xp}, Coins {stats.coins}");
            }
        }
    }

    // --- Helper Functions ---

    //checks if the email is in a valid format (basic check)
    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return false;
        
        return email.Contains("@") && email.Contains(".") && email.IndexOf("@") > 0;
    }
}