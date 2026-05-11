using UnityEngine;

namespace ActiveSaga.BossFight.Data
{
    [CreateAssetMenu(fileName = "DifficultyConfig", menuName = "BossFight/DifficultyConfig")]
    public class DifficultyConfig : ScriptableObject
    {
        public AnimationCurve spawnRateCurve;
        public AnimationCurve speedMultiplierCurve;
        public AnimationCurve countMultiplierCurve;

        public float GetSpeedMultiplier(int waveIndex) => speedMultiplierCurve.Evaluate(waveIndex);
        public float GetCountMultiplier(int waveIndex) => countMultiplierCurve.Evaluate(waveIndex);
    }
}
