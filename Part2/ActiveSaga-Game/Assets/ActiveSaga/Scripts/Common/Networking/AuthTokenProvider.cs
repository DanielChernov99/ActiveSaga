using UnityEngine;

namespace ActiveSaga.Common.Networking
{
    public class AuthTokenProvider : MonoBehaviour
    {
        public static AuthTokenProvider Instance { get; private set; }

        [Header("For Testing Only")]
        [SerializeField] private string tokenForTesting;

        public string Token { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (!string.IsNullOrWhiteSpace(tokenForTesting))
            {
                Token = tokenForTesting;
            }
        }

        public void SetToken(string token)
        {
            Token = token;
        }

        public bool HasToken()
        {
            return !string.IsNullOrWhiteSpace(Token);
        }
    }
}