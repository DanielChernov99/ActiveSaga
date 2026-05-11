using System.Collections.Generic;
using UnityEngine;

namespace ActiveSaga.BossFight.Core
{
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        [System.Serializable]
        public class PoolConfig
        {
            public string name;
            public GameObject prefab;
            public int initialSize = 10;
        }

        public List<PoolConfig> poolConfigs;
        
        private Dictionary<string, Queue<GameObject>> _poolDictionary = new Dictionary<string, Queue<GameObject>>();
        private Dictionary<string, GameObject> _prefabLookup = new Dictionary<string, GameObject>();
        private HashSet<GameObject> _currentlyPooled = new HashSet<GameObject>();
        
        private Transform _activeEnemiesParent;
        private Transform _activeProjectilesParent;
        private Transform _inactiveParent;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            CreateHierarchy();
            InitializePools();
        }

        private void CreateHierarchy()
        {
            _inactiveParent = new GameObject("InactivePool").transform;
            _inactiveParent.SetParent(transform);
            
            _activeEnemiesParent = new GameObject("ActiveEnemies").transform;
            _activeEnemiesParent.SetParent(transform);

            _activeProjectilesParent = new GameObject("ActiveProjectiles").transform;
            _activeProjectilesParent.SetParent(transform);
        }

        private void InitializePools()
        {
            if (poolConfigs == null) return;

            foreach (var config in poolConfigs)
            {
                if (config.prefab == null || string.IsNullOrEmpty(config.name)) continue;
                
                _prefabLookup[config.name] = config.prefab;
                Queue<GameObject> queue = new Queue<GameObject>();

                for (int i = 0; i < config.initialSize; i++)
                {
                    GameObject obj = CreateNewObject(config.prefab);
                    queue.Enqueue(obj);
                    _currentlyPooled.Add(obj);
                }

                _poolDictionary[config.name] = queue;
                Debug.Log($"Pool initialized: {config.name} with {config.initialSize} objects.");
            }
        }

        private GameObject CreateNewObject(GameObject prefab)
        {
            GameObject obj = Instantiate(prefab, _inactiveParent);
            obj.SetActive(false);
            return obj;
        }

        public GameObject SpawnFromPool(string poolName, Vector3 position, Quaternion rotation, bool isEnemy = true)
        {
            if (string.IsNullOrEmpty(poolName) || !_poolDictionary.ContainsKey(poolName))
            {
                Debug.LogError($"PoolManager: Pool '{poolName}' not found!");
                return null;
            }

            GameObject obj;
            if (_poolDictionary[poolName].Count > 0)
            {
                obj = _poolDictionary[poolName].Dequeue();
                _currentlyPooled.Remove(obj);
            }
            else
            {
                if (!_prefabLookup.ContainsKey(poolName)) return null;
                obj = CreateNewObject(_prefabLookup[poolName]);
                Debug.Log($"PoolManager: Expanding pool '{poolName}'");
            }

            // Reset state
            obj.transform.SetParent(isEnemy ? _activeEnemiesParent : _activeProjectilesParent);
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            
            if (_prefabLookup.ContainsKey(poolName))
                obj.transform.localScale = _prefabLookup[poolName].transform.localScale;

            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            obj.SetActive(true);
            return obj;
        }

        public void ReturnToPool(GameObject obj, string poolName)
        {
            if (obj == null) return;

            if (string.IsNullOrEmpty(poolName) || !_poolDictionary.ContainsKey(poolName))
            {
                Debug.LogWarning($"Trying to return object to non-existent pool: {poolName}. Destroying instead.");
                Destroy(obj);
                return;
            }

            if (_currentlyPooled.Contains(obj))
            {
                Debug.LogWarning($"Object {obj.name} is already in the pool!");
                return;
            }

            obj.SetActive(false);
            obj.transform.SetParent(_inactiveParent);
            _poolDictionary[poolName].Enqueue(obj);
            _currentlyPooled.Add(obj);
        }
    }
}

