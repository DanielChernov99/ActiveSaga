using System.Collections.Generic;
using UnityEngine;

namespace ActiveSaga.MainScreen.Logic
{
    public class ExplanationPanelsController : MonoBehaviour
    {
        [Header("Explanation Panels")]
        [SerializeField] private List<GameObject> explanationPanels = new List<GameObject>();

        private GameObject currentOpenPanel;

        private void Start()
        {
            HideAllExplanations();
        }

        public void ShowExplanation(GameObject panel)
        {
            if (panel == null)
            {
                Debug.LogWarning("Explanation panel is not assigned.");
                return;
            }

            HideAllExplanations();

            panel.SetActive(true);
            panel.transform.SetAsLastSibling();

            currentOpenPanel = panel;
        }

        public void HideExplanation(GameObject panel)
        {
            if (panel == null)
            {
                Debug.LogWarning("Explanation panel is not assigned.");
                return;
            }

            panel.SetActive(false);

            if (currentOpenPanel == panel)
            {
                currentOpenPanel = null;
            }
        }

        public void HideCurrentExplanation()
        {
            if (currentOpenPanel == null)
            {
                return;
            }

            currentOpenPanel.SetActive(false);
            currentOpenPanel = null;
        }

        public void HideAllExplanations()
        {
            foreach (GameObject panel in explanationPanels)
            {
                if (panel != null)
                {
                    panel.SetActive(false);
                }
            }

            currentOpenPanel = null;
        }
    }
}