using UnityEngine;
using System.Collections;
using System;

namespace ActiveSaga.BossFight
{
    public class BossFightManager : MonoBehaviour
    {
        [Header("HP Management")]
        [SerializeField] private float bossHP = 100f;
        [SerializeField] private float playerHP = 100f;

        [Header("Difficulty")]
        [SerializeField] private float difficultyMultiplier = 1.0f;

        [Header("Automation")]
        public GameObject firstWaveToStart;

        [Header("Current Wave Stats")]
        [SerializeField] private int successCount;
        [SerializeField] private int failCount;

        [Header("References")]
        [SerializeField] private FightUIManager uiManager;

        [Header("State")]
        [SerializeField] private BossWave currentWave;

        private const float SUCCESS_THRESHOLD = 0.8f;

        public float BossHP => bossHP;
        public float PlayerHP => playerHP;
        public float DifficultyMultiplier => difficultyMultiplier;

        private void Start()
        {
            Debug.Log("BossFightManager initialized.");
            if (uiManager == null) uiManager = FindFirstObjectByType<FightUIManager>();

            if (firstWaveToStart != null)
            {
                StartCoroutine(StartFirstWaveDelayed());
            }
        }

        private IEnumerator StartFirstWaveDelayed()
        {
            yield return new WaitForSeconds(3f);
            
            BossWave wave = firstWaveToStart != null ? firstWaveToStart.GetComponent<BossWave>() : null;
            if (wave != null)
            {
                yield return StartCoroutine(BossAttackSequence(wave));
            }
            else
            {
                Debug.LogError("First wave object does not have a BossWave component or is null.");
            }
        }

        private IEnumerator BossAttackSequence(BossWave wave)
        {
            GameObject boss = GameObject.Find("GiantBoss");
            if (boss != null)
            {
                Animator anim = boss.GetComponent<Animator>();
                if (anim != null)
                {
                    Debug.Log("Boss playing attack animation.");
                    anim.Play("Attack");
                    // Wait for animation or a fixed time
                    yield return new WaitForSeconds(2.5f); 
                }
            }
            
            StartWave(wave);
        }

        public void ReportSuccess()
        {
            successCount++;
            Debug.Log($"Success reported! Total Success: {successCount}");
        }

        public void ReportFailure()
        {
            failCount++;
            playerHP -= 2f; // Small penalty for missed skeleton as per requirements
            if (playerHP < 0) playerHP = 0;
            Debug.Log($"Failure reported! Total Fail: {failCount}. Player HP: {playerHP}");
        }

        /// <summary>
        /// Starts a specific wave.
        /// </summary>
        public void StartWave(BossWave wave)
        {
            if (wave == null)
            {
                Debug.LogWarning("Attempted to start a null wave.");
                return;
            }

            successCount = 0;
            failCount = 0;
            currentWave = wave;
            currentWave.StartWave(difficultyMultiplier);
            Debug.Log($"Wave started: {wave.name}");
            
            if (uiManager != null) uiManager.ShowFeedback($"Starting Wave...", 1.5f);
        }

        /// <summary>
        /// Ends the current wave and resolves success or failure.
        /// </summary>
        public void EndCurrentWave()
        {
            if (currentWave == null) return;

            currentWave.EndWave();
            
            int total = successCount + failCount;
            float successRate = total > 0 ? (float)successCount / total : 0f;

            Debug.Log($"Wave ending. Success Rate: {successRate:P0}");

            if (successRate >= SUCCESS_THRESHOLD)
            {
                OnWaveSuccess();
                if (uiManager != null) uiManager.ShowFeedback("Wave Cleared!", 3f);
            }
            else
            {
                OnWaveFailed();
                if (uiManager != null) uiManager.ShowFeedback("Wave Failed!", 3f);
            }

            // Increase difficulty after each wave
            difficultyMultiplier += 0.1f;
            Debug.Log($"Wave resolved. New difficulty multiplier: {difficultyMultiplier}");
            
            currentWave = null;
        }

        protected virtual void OnWaveSuccess()
        {
            Debug.Log("Wave Cleared! Player succeeded.");
            bossHP -= 20f; 
            if (bossHP <= 0) bossHP = 0;
        }

        protected virtual void OnWaveFailed()
        {
            Debug.Log("Wave Failed! Player failed the wave requirement.");
            playerHP -= 10f; 
            if (playerHP <= 0) playerHP = 0;
        }
    }
}
