using UnityEngine;

namespace ActiveSaga.MainScreen.Logic
{
    public class HowToPlayController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panelHowToPlay;

        private void Start()
        {
            HideHowToPlay();
        }

        public void ShowHowToPlay()
        {
            if (panelHowToPlay == null)
            {
                Debug.LogWarning("Panel How To Play is not assigned.");
                return;
            }

            panelHowToPlay.SetActive(true);
        }

        public void HideHowToPlay()
        {
            if (panelHowToPlay == null)
            {
                Debug.LogWarning("Panel How To Play is not assigned.");
                return;
            }

            panelHowToPlay.SetActive(false);
        }
    }
}