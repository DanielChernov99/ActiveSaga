using UnityEngine;
using UnityEngine.SceneManagement; 

public class MainSceneManager : MonoBehaviour
{
    //logout function, clears the token and returns to login scene
    public void OnLogoutButtonClicked()
    {
        PlayerPrefs.DeleteKey("AuthToken");
        PlayerPrefs.Save();
        Debug.Log("Logged out. Token deleted.");
        SceneManager.LoadScene("Login"); 
    }
}