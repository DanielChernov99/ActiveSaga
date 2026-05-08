using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
        private HashSet<GameObject> _activeEntities = new HashSet<GameObject>();
        
        private int _totalSpawnedThisWave = 0;
        private int _successfullyHandledThisWave = 0; 
        private int _playerHitCountThisWave = 0;

        public int ActiveEntitiesCount => _activeEntities.Count;

        private void OnEnable()
        {
            EventManager.Subscribe<EnemySpawnedEvent>(OnEntitySpawned);
            EventManager.Subscribe<EnemyDespawnedEvent>(OnEnemyDespawned);
            EventManager.Subscribe<ProjectileSpawnedEvent>(OnEntitySpawned);
            EventManager.Subscribe<ProjectileDespawnedEvent>(OnProjectileDespawned);
        }

        private void OnDisable()
        {
            EventManager.Unsubscribe<EnemySpawnedEvent>(OnEntitySpawned);
            EventManager.Unsubscribe<EnemyDespawnedEvent>(OnEnemyDespawned);
            EventManager.Unsubscribe<ProjectileSpawnedEvent>(OnEntitySpawned);
            EventManager.Unsubscribe<ProjectileDespawnedEvent>(OnProjectileDespawned);
            
            StopAllCoroutines();
        }

        private Coroutine _waveLoopRoutine;
        private bool _isWaveActive = false;

        private void Start()
        {
            if (bossSpawnPoint == null)
            {
                var boss = BossController.Instance;
                if (boss != null) bossSpawnPoint = boss.transform;
            }

            if (_waveLoopRoutine != null) StopCoroutine(_waveLoopRoutine);
            _waveLoopRoutine = StartCoroutine(WaveLoopRoutine());
        }

        private IEnumerator WaveLoopRoutine()
        {
            // Initial delay before the first wave
            yield return new WaitForSeconds(4f); 

            while (true) 
            {
                if (_isWaveActive)
                {
                    yield return null;
                    continue;
                }

                // Always generate dynamic waves to ensure alternating pattern
                WaveData currentWave = GenerateDynamicWave(currentWaveIndex, nextWaveType);
                
                // Toggle next wave type for the NEXT iteration
                nextWaveType = (nextWaveType == WaveType.Combat) ? WaveType.Dodge : WaveType.Combat;

                _isWaveActive = true;
                yield return StartCoroutine(PlayWave(currentWave));
                _isWaveActive = false;
                
                // Destroy the dynamically created WaveData asset to prevent leak
                if (currentWave != null) Destroy(currentWave);

                currentWaveIndex++;
                // Buffer between waves
                yield return new WaitForSeconds(2.0f); 
            }
        }

        [Header("Master Data (Dynamic Generator)")]
        [SerializeField] private List<EnemyData> enemyMasterList;
        [SerializeField] private List<ProjectileData> projectileMasterList;

        private WaveData GenerateDynamicWave(int index, WaveType type)
        {
            WaveData dynamicWave = ScriptableObject.CreateInstance<WaveData>();
            dynamicWave.waveName = $"Dynamic {type} Wave {index + 1}";
            dynamicWave.waveType = type;
            dynamicWave.steps = new List<WaveStep>();

            float countMult = difficultyConfig != null ? difficultyConfig.GetCountMultiplier(index) : 1f;
            
            // Boss Attack Start
            dynamicWave.steps.Add(new WaveStep 
            { 
                type = WaveStep.StepType.BossAnimation, 
                animationTrigger = "Attack",
                delayAfterStep = 1f 
            });

            if (type == WaveType.Combat)
            {
                int enemyCount = Mathf.RoundToInt(3 * countMult);
                for (int i = 0; i < enemyCount; i++)
                {
                    dynamicWave.steps.Add(new WaveStep 
                    { 
                        type = WaveStep.StepType.SpawnEnemy, 
                        enemyData = GetRandomEnemyData(),
                        spawnOffset = new Vector3(Random.Range(-3f, 3f), 0, Random.Range(4f, 8f)),
                        delayAfterStep = Random.Range(0.5f, 1.5f)
                    });
                }
            }
            else
            {
                // Dynamic range for projectile count (6-10) scaled by difficulty
                int baseCount = Random.Range(6, 11);
                int projectileCount = Mathf.RoundToInt(baseCount * countMult);
                
                for (int i = 0; i < projectileCount; i++)
                {
                    dynamicWave.steps.Add(new WaveStep 
                    { 
                        type = WaveStep.StepType.SpawnProjectile, 
                        projectileData = GetRandomProjectileData(),
                        spawnOffset = new Vector3(Random.Range(-3f, 3f), 1f, Random.Range(3f, 6f)),
                        delayAfterStep = Random.Range(0.5f, 1.2f)
                    });
                }
            }

            return dynamicWave;
        }

        private EnemyData GetRandomEnemyData()
        {
            if (enemyMasterList != null && enemyMasterList.Count > 0)
            {
                return enemyMasterList[Random.Range(0, enemyMasterList.Count)];
            }
            
            // Search existing configs as fallback
            if (waveConfigs != null)
            {
                foreach (var config in waveConfigs)
                {
                    if (config == null || config.steps == null) continue;
                    var step = config.steps.Find(s => s.enemyData != null);
                    if (step != null) return step.enemyData;
                }
            }
            return null;
        }

        private ProjectileData GetRandomProjectileData()
        {
            if (projectileMasterList != null && projectileMasterList.Count > 0)
            {
                return projectileMasterList[Random.Range(0, projectileMasterList.Count)];
            }

            if (waveConfigs != null)
            {
                foreach (var config in waveConfigs)
                {
                    if (config == null || config.steps == null) continue;
                    var step = config.steps.Find(s => s.projectileData != null);
                    if (step != null) return step.projectileData;
                }
            }
            return null;
        }

        private IEnumerator PlayWave(WaveData data)
        {
            if (data == null) yield break;

            _totalSpawnedThisWave = 0;
            _successfullyHandledThisWave = 0;
            _playerHitCountThisWave = 0;

            Debug.Log($"<color=cyan>Starting {data.waveType} Wave {currentWaveIndex + 1}: {data.waveName}</color>");
            EventManager.Trigger(new WaveStartedEvent { waveIndex = currentWaveIndex + 1, name = data.waveName });

            float speedMult = difficultyConfig != null ? difficultyConfig.GetSpeedMultiplier(currentWaveIndex) : 1f;

            // Step A & B: Trigger Boss Animation and Wait for completion
            var animationStep = data.steps.Find(s => s.type == WaveStep.StepType.BossAnimation);
            if (animationStep != null)
            {
                if (BossController.Instance != null) BossController.Instance.PlayAnimation(animationStep.animationTrigger);
                yield return new WaitForSeconds(2.0f); // Giant@UnarmedAttack01 length
            }

            // Step C: Spawn the wave entities
            foreach (var step in data.steps)
            {
                if (step.type == WaveStep.StepType.BossAnimation) continue;

                ExecuteStep(step, speedMult);
                yield return new WaitForSeconds(step.delayAfterStep / speedMult);
            }

            // Step D: WAIT COMPLETELY until ALL spawned entities are destroyed, deflected, or despawned
            float timeout = 45f; 
            float timer = 0f;

            while (timer < timeout)
            {
                // If we've spawned things and the arena is now clear
                if (_totalSpawnedThisWave > 0 && _activeEntities.Count == 0)
                {
                    break;
                }
                
                // If nothing was spawned (unlikely but safe)
                if (_totalSpawnedThisWave == 0 && timer > 2f)
                {
                    break;
                }

                timer += Time.deltaTime;
                yield return null;
            }

            if (_activeEntities.Count > 0)
            {
                ForceClearActiveEntities();
            }

            EvaluateWave(data);

            Debug.Log($"Wave {currentWaveIndex + 1} Cleared.");
            EventManager.Trigger(new WaveCompletedEvent { success = true });
        }

        private void EvaluateWave(WaveData data)
        {
            Debug.Log($"EvaluateWave called for {data.waveName}. Total Spawned: {_totalSpawnedThisWave}, Successfully Handled: {_successfullyHandledThisWave}");

            if (_totalSpawnedThisWave == 0) return;

            float successRate = (float)_successfullyHandledThisWave / _totalSpawnedThisWave;
            bool success = successRate >= 0.7f; 

            Debug.Log($"Wave {data.waveType} Result: Handled {_successfullyHandledThisWave}/{_totalSpawnedThisWave} ({successRate:P0}). Hit: {_playerHitCountThisWave}");

            if (success)
            {
                Debug.Log("<color=green>Wave Success! Damaging Boss.</color>");
                if (BossController.Instance != null)
                {
                    BossController.Instance.TakeDamage(100f); 
                }
                else
                {
                    Debug.LogError("[WaveManager] CANNOT DAMAGE BOSS: BossController.Instance is NULL!");
                }
            }
            else
            {
                Debug.Log("<color=red>Wave Failed. No Boss damage.</color>");
            }
        }

        private void ForceClearActiveEntities()
        {
            Debug.Log($"<color=orange>Force clearing {_activeEntities.Count} entities.</color>");
            List<GameObject> toClear = new List<GameObject>(_activeEntities);
            foreach (var obj in toClear)
            {
                if (obj == null) continue;
                
                var enemy = obj.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    enemy.Despawn(false);
                    continue;
                }

                var proj = obj.GetComponent<ProjectileController>();
                if (proj != null)
                {
                    proj.Despawn();
                    continue;
                }
                
                obj.SetActive(false);
            }
            _activeEntities.Clear();
        }

        private void ExecuteStep(WaveStep step, float speedMultiplier)
        {
            if (step == null) return;

            switch (step.type)
            {
                case WaveStep.StepType.SpawnEnemy:
                    SpawnEnemy(step.enemyData, step.spawnOffset, speedMultiplier);
                    break;
                case WaveStep.StepType.SpawnProjectile:
                    SpawnProjectile(step.projectileData, step.spawnOffset, speedMultiplier);
                    break;
                case WaveStep.StepType.BossAnimation:
                    if (BossController.Instance != null) BossController.Instance.PlayAnimation(step.animationTrigger);
                    break;
            }
        }

        private void SpawnEnemy(EnemyData data, Vector3 offset, float speedMultiplier)
        {
            if (data == null || PoolManager.Instance == null || bossSpawnPoint == null) return;
            
            Vector3 basePos = bossSpawnPoint.position;
            Vector3 forward = Vector3.forward;

            if (BossFightGameManager.Instance != null && BossFightGameManager.Instance.PlayerTransform != null)
            {
                forward = (BossFightGameManager.Instance.PlayerTransform.position - basePos).normalized;
                forward.y = 0; 
            }

            Vector3 right = Vector3.Cross(Vector3.up, forward);

            // Calculate spawn position in front of the boss, based on the player's direction
            Vector3 spawnPos = basePos + (forward * offset.z) + (right * offset.x) + (Vector3.up * offset.y);
            Quaternion spawnRot = Quaternion.LookRotation(forward);

            GameObject enemy = PoolManager.Instance.SpawnFromPool(data.enemyName, spawnPos, spawnRot, true);
            if (enemy != null)
            {
                _totalSpawnedThisWave++;
                var controller = enemy.GetComponent<EnemyController>();
                if (controller != null) controller.Initialize(data, speedMultiplier);
            }
        }

        private void SpawnProjectile(ProjectileData data, Vector3 offset, float speedMultiplier)
{
    Debug.Log("========== SpawnProjectile START ==========");

    if (data == null)
    {
        Debug.LogError("SpawnProjectile FAILED: ProjectileData is NULL!");
        return;
    }

    if (PoolManager.Instance == null || bossSpawnPoint == null)
        return;

    Vector3 basePos = bossSpawnPoint.position;

    bool hasPlayer = BossFightGameManager.Instance?.PlayerTransform != null;

    Vector3 playerPos = hasPlayer
        ? BossFightGameManager.Instance.PlayerTransform.position
        : basePos + Vector3.forward * 5f;

    float yOffset = offset.y;

    // ---------------------------
    // 🔥 FIX: proper head/feet targeting system
    // ---------------------------
    bool targetHead = false;

    if (data.projectileName == "DodgeLog")
    {
        targetHead = Random.value > 0.5f;
    }
    else
    {
        // חשוב מאוד: נותן חלק מהפרויקטים לראש גם אם לא מוגדר
        targetHead = Random.value > 0.6f;
    }

    float targetHeight = targetHead ? 1.7f : 0.7f;

    Vector3 targetPos = playerPos + Vector3.up * targetHeight;

    Vector3 direction = (targetPos - basePos).normalized;
    if (direction.sqrMagnitude < 0.001f) direction = Vector3.forward;

    Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
    if (right.sqrMagnitude < 0.001f) right = Vector3.right;

    Vector3 spawnPos =
        basePos +
        direction * offset.z +
        right * offset.x +
        Vector3.up * yOffset;

    Quaternion spawnRot = Quaternion.LookRotation(direction);

    GameObject projectile =
        PoolManager.Instance.SpawnFromPool(
            data.projectileName,
            spawnPos,
            spawnRot,
            false
        );

    if (projectile == null)
    {
        Debug.LogError($"Pool returned NULL for {data.projectileName}");
        return;
    }

    _totalSpawnedThisWave++;

    var controller = projectile.GetComponent<ProjectileController>();

    if (controller == null)
    {
        Debug.LogError($"Missing ProjectileController on {projectile.name}");
        return;
    }

    controller.Initialize(data, speedMultiplier);

    Debug.Log($"Spawned {data.projectileName} -> TargetHead: {targetHead}");
    Debug.Log("========== SpawnProjectile END ==========");
}

        private void OnEntitySpawned(EnemySpawnedEvent e) 
        { 
            if (e.enemy != null && !_activeEntities.Contains(e.enemy)) 
            {
                _activeEntities.Add(e.enemy);
            }
        }
        
        private void OnEntitySpawned(ProjectileSpawnedEvent e) 
        { 
            if (e.projectile != null && !_activeEntities.Contains(e.projectile)) 
            {
                _activeEntities.Add(e.projectile);
            }
        }
        
        private void OnEnemyDespawned(EnemyDespawnedEvent e) 
        { 
            if (e.enemy != null)
            {
                _activeEntities.Remove(e.enemy);
                if (e.wasKilledByPlayer) _successfullyHandledThisWave++;
                else _playerHitCountThisWave++; 
            } 
        }

        private void OnProjectileDespawned(ProjectileDespawnedEvent e) 
        { 
            if (e.projectile != null) 
            {
                _activeEntities.Remove(e.projectile);
                if (e.wasDodged) _successfullyHandledThisWave++;
                else if (e.wasHitPlayer) _playerHitCountThisWave++;
            } 
        }

        private void Update()
        {
            if (Time.frameCount % 30 == 0 && _activeEntities.Count > 0)
            {
                // Safety cleanup for destroyed or inactive objects
                _activeEntities.RemoveWhere(item => item == null || !item.activeInHierarchy);
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