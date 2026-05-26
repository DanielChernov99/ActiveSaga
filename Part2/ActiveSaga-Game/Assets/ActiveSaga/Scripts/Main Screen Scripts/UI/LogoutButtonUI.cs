using ActiveSaga.Common.GameSession;
using ActiveSaga.Common.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ActiveSaga.MainScreen.UI
{
    public class LogoutButtonUI : MonoBehaviour
    {
        [Header("Button")]
        [SerializeField] private Button logoutButton;

        [Header("Scene")]
        [SerializeField] private string loginSceneName = "Login";

        private void Awake()
        {
            if (logoutButton == null)
            {
                logoutButton = GetComponent<Button>();
            }
        }

        private void OnEnable()
        {
            if (logoutButton != null)
            {
                logoutButton.onClick.AddListener(Logout);
            }
        }

        private void OnDisable()
        {
            if (logoutButton != null)
            {
                logoutButton.onClick.RemoveListener(Logout);
            }
        }

        private void Logout()
        {
            PlayerApiService.ClearToken();

            if (AuthTokenProvider.Instance != null)
            {
                AuthTokenProvider.Instance.ClearToken();
            }

            GameLaunchData.Clear();

            SceneManager.LoadScene(loginSceneName);
        }
    }
}