using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ActiveSaga.Common.GameSession;
using ActiveSaga.BossFight.Core;
using ActiveSaga.BossFight.Data;
using ActiveSaga.BossFight.Entities;

namespace ActiveSaga.BossFight.Waves
{
    public class WaveManager : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private List<WaveData> waveConfigs;
        [SerializeField] private DifficultyConfig difficultyConfig;

        [Header("Settings")]
        [SerializeField] private Transform bossSpawnPoint;

        [Header("Runtime State")]
        [SerializeField] private int currentWaveIndex = 0;
        [SerializeField] private WaveType nextWaveType = WaveType.Combat;

        [Header("Master Data (Dynamic Generator)")]
        [SerializeField] private List<EnemyData> enemyMasterList;
        [SerializeField] private List<ProjectileData> projectileMasterList;

        private Coroutine waveLoopRoutine;
        private bool isWaveActive;
        private WaveData currentDynamicWave;

        private DynamicWaveGenerator dynamicWaveGenerator;
        private WaveEntitySpawner entitySpawner;
        private WaveEntityTracker entityTracker;
        private WaveEvaluator waveEvaluator;

        public int ActiveEntitiesCount
        {
            get
            {
                return entityTracker != null ? entityTracker.ActiveEntitiesCount : 0;
            }
        }

        private void Awake()
        {
            InitializeHelpers();
        }

        private void OnEnable()
        {
            InitializeHelpers();

            if (entityTracker != null)
            {
                entityTracker.Subscribe();
            }
        }

        private void OnDisable()
        {
            if (entityTracker != null)
            {
                entityTracker.Unsubscribe();
            }

            StopAllCoroutines();
            waveLoopRoutine = null;
        }

        private void Start()
        {
            ResolveBossSpawnPoint();

            InitializeHelpers();

            if (entitySpawner != null)
            {
                entitySpawner.SetBossSpawnPoint(bossSpawnPoint);
            }

            if (waveLoopRoutine != null)
            {
                StopCoroutine(waveLoopRoutine);
            }

            waveLoopRoutine = StartCoroutine(WaveLoopRoutine());
        }

        private void InitializeHelpers()
        {
            if (dynamicWaveGenerator == null)
            {
                dynamicWaveGenerator = new DynamicWaveGenerator(
                    waveConfigs,
                    difficultyConfig,
                    enemyMasterList,
                    projectileMasterList
                );
            }

            if (entitySpawner == null)
            {
                entitySpawner = new WaveEntitySpawner(
                    bossSpawnPoint,
                    CanRunWaveLogic
                );
            }

            if (entityTracker == null)
            {
                entityTracker = new WaveEntityTracker(
                    CanCountWaveStats
                );
            }

            if (waveEvaluator == null)
            {
                waveEvaluator = new WaveEvaluator();
            }
        }

        private void ResolveBossSpawnPoint()
        {
            if (bossSpawnPoint != null)
            {
                return;
            }

            BossController boss = BossController.Instance;

            if (boss != null)
            {
                bossSpawnPoint = boss.transform;
            }
        }

        private IEnumerator WaveLoopRoutine()
        {
            yield return new WaitForSeconds(4f);
            yield return WaitWhilePaused();

            if (!CanSessionContinue())
            {
                waveLoopRoutine = null;
                yield break;
            }

            while (CanSessionContinue())
            {
                yield return WaitWhilePaused();

                if (!CanSessionContinue())
                {
                    break;
                }

                if (isWaveActive)
                {
                    yield return null;
                    continue;
                }

                WaveData currentWave = dynamicWaveGenerator.Generate(
                    currentWaveIndex,
                    nextWaveType
                );

                currentDynamicWave = currentWave;

                nextWaveType = nextWaveType == WaveType.Combat
                    ? WaveType.Dodge
                    : WaveType.Combat;

                isWaveActive = true;

                yield return StartCoroutine(PlayWave(currentWave));

                isWaveActive = false;

                if (currentWave != null)
                {
                    Destroy(currentWave);
                }

                if (currentDynamicWave == currentWave)
                {
                    currentDynamicWave = null;
                }

                if (!CanSessionContinue())
                {
                    break;
                }

                currentWaveIndex++;

                yield return new WaitForSeconds(2.0f);
                yield return WaitWhilePaused();

                if (!CanSessionContinue())
                {
                    break;
                }
            }

            waveLoopRoutine = null;
        }

        private IEnumerator PlayWave(WaveData data)
        {
            if (data == null)
            {
                yield break;
            }

            yield return WaitWhilePaused();

            if (!CanSessionContinue())
            {
                yield break;
            }

            entityTracker.ResetWaveCounters();

            if (GameSessionManager.Instance != null)
            {
                GameSessionManager.Instance.StartGameplayTimerIfNeeded();
            }

            Debug.Log($"<color=cyan>Starting {data.waveType} Wave {currentWaveIndex + 1}: {data.waveName}</color>");

            EventManager.Trigger(new WaveStartedEvent
            {
                waveIndex = currentWaveIndex + 1,
                name = data.waveName
            });

            float speedMult = difficultyConfig != null
                ? difficultyConfig.GetSpeedMultiplier(currentWaveIndex)
                : 1f;

            if (speedMult <= 0f)
            {
                speedMult = 1f;
            }

            WaveStep animationStep = data.steps.Find(s => s.type == WaveStep.StepType.BossAnimation);

            if (animationStep != null)
            {
                yield return WaitWhilePaused();

                if (!CanSessionContinue())
                {
                    yield break;
                }

                if (BossController.Instance != null)
                {
                    BossController.Instance.PlayAnimation(animationStep.animationTrigger);
                }

                yield return new WaitForSeconds(2.0f);
                yield return WaitWhilePaused();

                if (!CanSessionContinue())
                {
                    yield break;
                }
            }

            foreach (WaveStep step in data.steps)
            {
                yield return WaitWhilePaused();

                if (!CanSessionContinue())
                {
                    yield break;
                }

                if (step.type == WaveStep.StepType.BossAnimation)
                {
                    continue;
                }

                entitySpawner.ExecuteStep(step, speedMult);

                yield return new WaitForSeconds(step.delayAfterStep / speedMult);
                yield return WaitWhilePaused();

                if (!CanSessionContinue())
                {
                    yield break;
                }
            }

            float timeout = 45f;
            float timer = 0f;

            while (timer < timeout)
            {
                yield return WaitWhilePaused();

                if (!CanSessionContinue())
                {
                    yield break;
                }

                if (entityTracker.TotalSpawnedThisWave > 0 &&
                    entityTracker.ActiveEntitiesCount == 0)
                {
                    break;
                }

                if (entityTracker.TotalSpawnedThisWave == 0 && timer > 2f)
                {
                    break;
                }

                timer += Time.deltaTime;
                yield return null;
            }

            yield return WaitWhilePaused();

            if (!CanSessionContinue())
            {
                yield break;
            }

            if (entityTracker.ActiveEntitiesCount > 0)
            {
                entityTracker.ForceClearActiveEntities();
            }

            if (!CanSessionContinue())
            {
                yield break;
            }

            bool waveSuccess = waveEvaluator.EvaluateWave(
                data,
                entityTracker.TotalSpawnedThisWave,
                entityTracker.SuccessfullyHandledThisWave,
                entityTracker.PlayerHitCountThisWave
            );

            Debug.Log($"Wave {currentWaveIndex + 1} Cleared.");

            EventManager.Trigger(new WaveCompletedEvent
            {
                success = waveSuccess
            });
        }

        public void StopWavesAfterGameEnded()
        {
            Debug.Log("<color=orange>[WaveManager] Stopping waves because game ended.</color>");

            if (waveLoopRoutine != null)
            {
                StopCoroutine(waveLoopRoutine);
                waveLoopRoutine = null;
            }

            isWaveActive = false;

            if (entityTracker != null)
            {
                entityTracker.ForceClearActiveEntitiesWithoutStats();
            }

            if (currentDynamicWave != null)
            {
                Destroy(currentDynamicWave);
                currentDynamicWave = null;
            }
        }

        private IEnumerator WaitWhilePaused()
        {
            while (IsSessionPaused())
            {
                yield return null;
            }
        }

        private bool IsSessionPaused()
        {
            return GameSessionManager.Instance != null &&
                   GameSessionManager.Instance.State == GameSessionState.Paused;
        }

        private bool CanSessionContinue()
        {
            if (GameSessionManager.Instance == null)
            {
                return true;
            }

            GameSessionState state = GameSessionManager.Instance.State;

            return state != GameSessionState.WaitingForServer &&
                   state != GameSessionState.Ended;
        }

        private bool CanRunWaveLogic()
        {
            return CanSessionContinue() && !IsSessionPaused();
        }

        private bool CanCountWaveStats()
        {
            return CanSessionContinue();
        }

        private void Update()
        {
            if (IsSessionPaused())
            {
                return;
            }

            if (Time.frameCount % 30 == 0 && entityTracker != null)
            {
                entityTracker.CleanupInactiveEntities();
            }
        }

        private void OnDrawGizmos()
        {
            if (bossSpawnPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(bossSpawnPoint.position, 1f);
            }
        }
    }
}