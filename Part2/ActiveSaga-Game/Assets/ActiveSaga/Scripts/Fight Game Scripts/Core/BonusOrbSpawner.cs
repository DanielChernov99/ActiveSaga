using System.Collections;
using UnityEngine;
using ActiveSaga.Common.GameSession;

namespace ActiveSaga.BossFight.Core
{
    public class BonusOrbSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject orbPrefab;
        [SerializeField] private Transform playerCamera;
        [SerializeField] private Transform orbParent;

        [Header("Spawn Timing")]
        [SerializeField] private float firstSpawnDelay = 5f;
        [SerializeField] private float minSpawnInterval = 6f;
        [SerializeField] private float maxSpawnInterval = 9f;

        [Header("Spawn Position")]
        [SerializeField] private float forwardDistance = 1.0f;
        [SerializeField] private float minSideDistance = 0.55f;
        [SerializeField] private float maxSideDistance = 0.95f;
        [SerializeField] private float minHeightOffsetFromCamera = -0.45f;
        [SerializeField] private float maxHeightOffsetFromCamera = 0.05f;

        private Coroutine spawnRoutine;
        private GameObject activeOrb;

        private void OnEnable()
        {
            spawnRoutine = StartCoroutine(SpawnRoutine());
        }

        private void OnDisable()
        {
            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;
            }
        }

        private IEnumerator SpawnRoutine()
        {
            yield return new WaitForSeconds(firstSpawnDelay);

            while (true)
            {
                yield return WaitWhilePausedOrInactive();

                if (CanRun() && orbPrefab != null && activeOrb == null)
                {
                    SpawnOrb();
                }

                float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
                float timer = 0f;

                while (timer < waitTime)
                {
                    yield return WaitWhilePausedOrInactive();
                    timer += Time.deltaTime;
                    yield return null;
                }
            }
        }

        private void SpawnOrb()
        {
            Transform cameraTransform = ResolvePlayerCamera();
            if (cameraTransform == null)
            {
                return;
            }

            Vector3 forward = cameraTransform.forward;
            forward.y = 0f;
            forward.Normalize();

            if (forward.sqrMagnitude < 0.001f)
            {
                forward = Vector3.forward;
            }

            Vector3 right = cameraTransform.right;
            right.y = 0f;
            right.Normalize();

            if (right.sqrMagnitude < 0.001f)
            {
                right = Vector3.right;
            }

            float side = Random.value < 0.5f ? -1f : 1f;
            float sideDistance = Random.Range(minSideDistance, maxSideDistance) * side;
            float heightOffset = Random.Range(minHeightOffsetFromCamera, maxHeightOffsetFromCamera);

            Vector3 spawnPosition =
                cameraTransform.position +
                forward * forwardDistance +
                right * sideDistance +
                Vector3.up * heightOffset;

            activeOrb = Instantiate(orbPrefab, spawnPosition, Quaternion.identity, orbParent);
        }

        private Transform ResolvePlayerCamera()
        {
            if (playerCamera != null)
            {
                return playerCamera;
            }

            if (BossFightGameManager.Instance != null && BossFightGameManager.Instance.PlayerCamera != null)
            {
                playerCamera = BossFightGameManager.Instance.PlayerCamera.transform;
                return playerCamera;
            }

            if (Camera.main != null)
            {
                playerCamera = Camera.main.transform;
                return playerCamera;
            }

            return null;
        }

        private IEnumerator WaitWhilePausedOrInactive()
        {
            while (!CanRun())
            {
                yield return null;
            }
        }

        private bool CanRun()
        {
            if (GameSessionManager.Instance == null)
            {
                return true;
            }

            GameSessionState state = GameSessionManager.Instance.State;

            return state != GameSessionState.Paused &&
                   state != GameSessionState.WaitingForServer &&
                   state != GameSessionState.Ended;
        }
    }
}
