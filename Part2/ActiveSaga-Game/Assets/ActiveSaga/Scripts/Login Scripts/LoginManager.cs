using UnityEngine;
using UnityEngine.Networking; // Prepared for future API communication
using System.Collections;

public class LoginManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject loginPanel;
    public GameObject registerPanel;

    // --- Navigation System: Switching Screens ---

    public void OpenRegisterScreen()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
    }

    public void OpenLoginScreen()
    {
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);
    }

    // --- Future Preparation: API Communication ---

    public void SubmitLogin(string username, string password)
    {
        Debug.Log("Starting Login Process...");
        StartCoroutine(SendLoginRequest(username, password));
    }

    private IEnumerator SendLoginRequest(string username, string password)
    {
        // Future server URL and JSON payload will be placed here
        // UnityWebRequest request = UnityWebRequest.Post("https://your-server.com/api/login", form);
        // yield return request.SendWebRequest();

        Debug.Log($"Placeholder: Sending {username} to Server...");
        yield return null; 
    }
}