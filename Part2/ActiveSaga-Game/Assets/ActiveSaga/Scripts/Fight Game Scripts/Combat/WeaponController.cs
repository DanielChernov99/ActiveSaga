using UnityEngine;
using ActiveSaga.BossFight.Data;

namespace ActiveSaga.BossFight.Combat
{
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] private HandType handType;
        
        private Vector3 lastPosition;
        private Vector3 currentVelocity;

        public HandType Hand => handType;
        public Vector3 Velocity => currentVelocity;

        private void Start()
        {
            lastPosition = transform.position;
        }

        private void FixedUpdate()
        {
            currentVelocity = (transform.position - lastPosition) / Time.fixedDeltaTime;
            lastPosition = transform.position;
        }

        public bool IsHitValid(float requiredThreshold)
        {
            return currentVelocity.magnitude >= requiredThreshold;
        }
    }
}