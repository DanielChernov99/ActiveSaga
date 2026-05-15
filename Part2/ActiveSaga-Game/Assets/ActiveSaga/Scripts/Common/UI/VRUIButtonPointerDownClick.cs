using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ActiveSaga.Common.UI
{
    [RequireComponent(typeof(Button))]
    public class VRUIButtonPointerDownClick : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private float clickCooldown = 0.35f;
        [SerializeField] private bool debugLogs = true;

        private Button button;
        private float lastClickTime = -999f;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (button == null)
            {
                return;
            }

            if (!button.interactable)
            {
                return;
            }

            if (Time.unscaledTime - lastClickTime < clickCooldown)
            {
                return;
            }

            lastClickTime = Time.unscaledTime;

            if (debugLogs)
            {
                Debug.Log($"{gameObject.name}: VR PointerDown converted to Button Click");
            }

            button.onClick.Invoke();
        }
    }
}