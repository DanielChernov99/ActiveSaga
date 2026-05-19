using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Text;
using System.Collections;
using UnityEngine.SceneManagement;
using ActiveSaga.Common.Networking;

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

[System.Serializable]
public class LoginResponse
{
    public string message;
    public string accountId;
    public string username;
    public string token;
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
    [Header("API")]
    [SerializeField] private string authBaseUrl = "http://localhost:3000/api/auth";
    [SerializeField] private string playerBaseUrl = "http://localhost:3000/api/player";

    [Header("Scenes")]
    [SerializeField] private string mainSceneName = "Main New";

    [Header("Error Displays")]
    [SerializeField] private TextMeshProUGUI loginErrorText;
    [SerializeField] private TextMeshProUGUI registerErrorText;

    [Header("Login UI Fields")]
    [SerializeField] private TMP_InputField loginIdentifierInput;
    [SerializeField] private TMP_InputField loginPasswordInput;

    [Header("Register UI Fields")]
    [SerializeField] private TMP_InputField regEmailInput;
    [SerializeField] private TMP_InputField regUsernameInput;
    [SerializeField] private TMP_InputField regPasswordInput;
    [SerializeField] private TMP_InputField regFirstNameInput;
    [SerializeField] private TMP_InputField regLastNameInput;

    private void Start()
    {
        MigrateOldTokenIfNeeded();

        if (PlayerApiService.HasToken())
        {
            Debug.Log("Found saved token, validating...");
            StartCoroutine(ValidateSavedTokenCoroutine(true));
        }
    }

    public void OnLoginButtonClicked()
    {
        ClearErrors();

        string identifier = loginIdentifierInput.text.Trim();
        string password = loginPasswordInput.text;

        if (string.IsNullOrEmpty(identifier) || string.IsNullOrEmpty(password))
        {
            ShowError("Please fill in all fields.", loginErrorText);
            return;
        }

        StartCoroutine(LoginCoroutine(identifier, password));
    }

    public void OnRegisterButtonClicked()
    {
        ClearErrors();

        string email = regEmailInput.text.Trim();
        string username = regUsernameInput.text.Trim();
        string password = regPasswordInput.text;
        string firstName = regFirstNameInput.text.Trim();
        string lastName = regLastNameInput.text.Trim();

        if (string.IsNullOrEmpty(email) ||
            string.IsNullOrEmpty(username) ||
            string.IsNullOrEmpty(password) ||
            string.IsNullOrEmpty(firstName) ||
            string.IsNullOrEmpty(lastName))
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

    private IEnumerator LoginCoroutine(string identifier, string password)
    {
        LoginRequest loginData = new LoginRequest
        {
            identifier = identifier,
            password = password
        };

        string jsonData = JsonUtility.ToJson(loginData);
        string url = authBaseUrl.TrimEnd('/') + "/login";

        using (UnityWebRequest request = CreatePostRequest(url, jsonData))
        {
            yield return request.SendWebRequest();

            if (HasRequestError(request))
            {
                string errorMsg = ExtractServerError(request, "An error occurred during login.");
                Debug.LogError("Login Error:\n" + errorMsg + "\nServer Response: " + request.downloadHandler.text);
                ShowError(errorMsg, loginErrorText);
                yield break;
            }

            LoginResponse responseData = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);

            if (responseData == null || string.IsNullOrEmpty(responseData.token))
            {
                ShowError("Login succeeded but token was missing.", loginErrorText);
                yield break;
            }

            PlayerApiService.SaveToken(responseData.token);

            Debug.Log("Login successful. Token saved for Main scene.");

            loginIdentifierInput.text = "";
            loginPasswordInput.text = "";

            SceneManager.LoadScene(mainSceneName);
        }
    }

    private IEnumerator RegisterCoroutine(
        string email,
        string username,
        string password,
        string firstName,
        string lastName)
    {
        RegisterRequest registerData = new RegisterRequest
        {
            email = email,
            username = username,
            password = password,
            firstName = firstName,
            lastName = lastName
        };

        string jsonData = JsonUtility.ToJson(registerData);
        string url = authBaseUrl.TrimEnd('/') + "/register";

        using (UnityWebRequest request = CreatePostRequest(url, jsonData))
        {
            yield return request.SendWebRequest();

            if (HasRequestError(request))
            {
                string errorMsg = ExtractServerError(request, "An error occurred during registration.");
                Debug.LogError("Registration Error:\n" + errorMsg + "\nServer Response: " + request.downloadHandler.text);
                ShowError(errorMsg, registerErrorText);
                yield break;
            }

            RegisterResponse responseData = JsonUtility.FromJson<RegisterResponse>(request.downloadHandler.text);

            if (responseData == null || string.IsNullOrEmpty(responseData.token))
            {
                ShowError("Registration succeeded but token was missing.", registerErrorText);
                yield break;
            }

            PlayerApiService.SaveToken(responseData.token);

            Debug.Log("Registration successful. Token saved for Main scene.");

            regEmailInput.text = "";
            regUsernameInput.text = "";
            regPasswordInput.text = "";
            regFirstNameInput.text = "";
            regLastNameInput.text = "";

            SceneManager.LoadScene(mainSceneName);
        }
    }

    private IEnumerator ValidateSavedTokenCoroutine(bool shouldNavigateOnSuccess)
    {
        string token = PlayerApiService.GetToken();

        if (string.IsNullOrEmpty(token))
        {
            yield break;
        }

        string url = playerBaseUrl.TrimEnd('/') + "/me";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (HasRequestError(request))
            {
                Debug.LogWarning("Saved token is invalid or server is unreachable. Clearing token.");
                PlayerApiService.ClearToken();
                yield break;
            }

            Debug.Log("Saved token is valid.");

            if (shouldNavigateOnSuccess)
            {
                SceneManager.LoadScene(mainSceneName);
            }
        }
    }

    private UnityWebRequest CreatePostRequest(string url, string jsonData)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        return request;
    }

    private bool HasRequestError(UnityWebRequest request)
    {
        return request.result == UnityWebRequest.Result.ConnectionError ||
               request.result == UnityWebRequest.Result.ProtocolError ||
               request.result == UnityWebRequest.Result.DataProcessingError;
    }

    private string ExtractServerError(UnityWebRequest request, string fallbackMessage)
    {
        if (request == null || request.downloadHandler == null)
        {
            return fallbackMessage;
        }

        string body = request.downloadHandler.text;

        if (string.IsNullOrEmpty(body))
        {
            return fallbackMessage;
        }

        try
        {
            ErrorResponse serverError = JsonUtility.FromJson<ErrorResponse>(body);

            if (serverError != null && !string.IsNullOrEmpty(serverError.message))
            {
                return serverError.message;
            }
        }
        catch
        {
            return fallbackMessage;
        }

        return fallbackMessage;
    }

    private void ShowError(string errorMessage, TextMeshProUGUI displayTarget)
    {
        if (displayTarget != null)
        {
            displayTarget.text = errorMessage;
        }
    }

    private void ClearErrors()
    {
        if (loginErrorText != null)
        {
            loginErrorText.text = "";
        }

        if (registerErrorText != null)
        {
            registerErrorText.text = "";
        }
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return false;
        }

        return email.Contains("@") && email.Contains(".") && email.IndexOf("@") > 0;
    }

    private void MigrateOldTokenIfNeeded()
    {
        const string oldTokenKey = "AuthToken";

        if (PlayerApiService.HasToken())
        {
            return;
        }

        if (!PlayerPrefs.HasKey(oldTokenKey))
        {
            return;
        }

        string oldToken = PlayerPrefs.GetString(oldTokenKey, "");

        if (!string.IsNullOrEmpty(oldToken))
        {
            PlayerApiService.SaveToken(oldToken);
        }

        PlayerPrefs.DeleteKey(oldTokenKey);
        PlayerPrefs.Save();
    }
}