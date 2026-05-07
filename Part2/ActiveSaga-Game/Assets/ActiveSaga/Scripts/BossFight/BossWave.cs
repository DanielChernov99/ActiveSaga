using UnityEngine;

namespace ActiveSaga.BossFight
{
    /// <summary>
    /// Base class for all boss fight waves (e.g., Melee, Dodge).
    /// </summary>
    public abstract class BossWave : MonoBehaviour
    {
        [Header("Wave Stats")]
        [SerializeField] protected int successCount;
        [SerializeField] protected int failCount;
        [SerializeField] protected int totalTargets;

        public abstract void StartWave(float difficultyMultiplier);
        public abstract void EndWave();
        
        /// <summary>
        /// Returns the success rate of the wave as a normalized float (0.0 to 1.0).
        /// </summary>
        public virtual float GetSuccessRate()
        {
            if (totalTargets == 0) return 0f;
            return (float)successCount / totalTargets;
        }

        public virtual void ReportSuccess()
        {
            successCount++;
            CheckWaveCompletion();
        }

        public virtual void ReportFailure()
        {
            failCount++;
            CheckWaveCompletion();
        }

        protected virtual void CheckWaveCompletion()
        {
            if (successCount + failCount >= totalTargets)
            {
                FindObjectOfType<BossFightManager>().EndCurrentWave();
            }
        }
    }
}
