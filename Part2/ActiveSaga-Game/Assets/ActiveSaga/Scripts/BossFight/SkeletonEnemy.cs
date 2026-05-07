using UnityEngine;

namespace ActiveSaga.BossFight
{
    public class SkeletonEnemy : MonoBehaviour
    {
        public enum SkeletonType { Red, Blue }

        [Header("Settings")]
        public SkeletonType type;
        public float moveSpeed = 2f;
        
        private Transform target;
        private BossFightManager manager;
        private bool isDead = false;

        public void Initialize(SkeletonType type, float speed, Transform playerTarget, BossFightManager bossManager)
        {
            this.type = type;
            this.moveSpeed = speed;
            this.target = playerTarget;
            this.manager = bossManager;

            // Simple visual indicator if materials aren't set up yet
            var renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = (type == SkeletonType.Red) ? Color.red : Color.blue;
            }
        }

        private void Update()
        {
            if (isDead) return;

            // Force find camera if target is missing or lost
            if (target == null)
            {
                GameObject camObj = GameObject.Find("Main Camera");
                if (camObj != null) target = camObj.transform;
                else
                {
                    Camera mainCam = Camera.main;
                    if (mainCam != null) target = mainCam.transform;
                }
            }

            if (target == null) return;

            // 1. Face the player (LookAt)
            Vector3 lookTarget = new Vector3(target.position.x, transform.position.y, target.position.z);
            transform.LookAt(lookTarget);

            // 2. Move towards target (XZ only to stay on floor)
            Vector3 targetPos = new Vector3(target.position.x, transform.position.y, target.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isDead) return;

            // Check if hit by correct hand
            if (type == SkeletonType.Red && other.CompareTag("RightHand"))
            {
                HandleDeath(true);
            }
            else if (type == SkeletonType.Blue && other.CompareTag("LeftHand"))
            {
                HandleDeath(true);
            }
            // Check if reached player camera hitbox
            else if (other.CompareTag("PlayerHitbox") || other.CompareTag("MainCamera") || other.name.Contains("Camera"))
            {
                HandleDeath(false);
            }
        }

        private void HandleDeath(bool wasPunchedCorrectly)
        {
            isDead = true;

            if (wasPunchedCorrectly)
            {
                manager.ReportSuccess();
            }
            else
            {
                manager.ReportFailure();
            }

            Destroy(gameObject);
        }
    }
}
