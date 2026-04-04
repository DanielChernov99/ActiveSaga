using UnityEngine;
using UnityEngine.Networking;
using TMPro; 
using System.Text;
using System.Collections;

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
    public PlayerStats playerStats;
}



public class AuthManager : MonoBehaviour
{
    private string baseUrl = "http://localhost:3000/api/auth";

    [Header("Login UI Fields")]
    public TMP_InputField loginIdentifierInput; 
    public TMP_InputField loginPasswordInput;

    [Header("Register UI Fields")]
    public TMP_InputField regEmailInput;
    public TMP_InputField regUsernameInput;
    public TMP_InputField regPasswordInput;
    public TMP_InputField regFirstNameInput;
    public TMP_InputField regLastNameInput;


    // --- Login Functions ---
    public void OnLoginButtonClicked()
    {
        string identifier = loginIdentifierInput.text;
        string password = loginPasswordInput.text;

        if (string.IsNullOrEmpty(identifier) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("⚠️ Error: Missing username or password");
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
                Debug.LogError($"❌ Login Error: {request.error}\nServer Response: {request.downloadHandler.text}");
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
            }
        }
    }

    // --- Register Functions ---
    public void OnRegisterButtonClicked()
    {
        string email = regEmailInput.text;
        string username = regUsernameInput.text;
        string password = regPasswordInput.text;
        string firstName = regFirstNameInput.text;
        string lastName = regLastNameInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
        {
            Debug.LogWarning("⚠️ Error: Please fill in all required fields");
            return;
        }

        if (!IsValidEmail(email))
        {
            Debug.LogWarning("⚠️ Error: Please enter a valid email address (must contain '@' and '.')");
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
                Debug.LogError($"❌ Registration Error: {request.error}\nServer Response: {request.downloadHandler.text}");
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